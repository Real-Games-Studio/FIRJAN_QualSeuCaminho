using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace _4._NFC_Firjan.Scripts.Server
{
	public class ServerComunication : MonoBehaviour
	{
		/// <summary>
		/// Precisa ser colocado para receber do Json em cada aplicação.
		/// </summary>
		public string Ip;
		
		/// <summary>
		/// Precisa ser colocado para receber do Json em cada aplicação.
		/// </summary>
		public int Port;

		private HttpClient _client;

		private void Awake()
		{
			// initialize http client immediately
			_client = new HttpClient();

			// Try to read configuration from GameDataLoader. It may not be ready yet (order of Awake calls),
			// so handle nulls safely and subscribe to updates if necessary.
			if (GameDataLoader.instance != null && GameDataLoader.instance.loadedConfig != null)
			{
				Ip = GameDataLoader.instance.loadedConfig.serverIP;
				Port = GameDataLoader.instance.loadedConfig.serverPort;
			}
			else if (GameDataLoader.instance != null)
			{
				// subscribe to be notified when the game data/config is ready
				GameDataLoader.instance.OnGameDataUpdated += OnGameDataUpdated;
				// in case the loader becomes available a bit later, also start a short waiter
				StartCoroutine(WaitForConfig());
			}
			else
			{
				// If GameDataLoader.instance is null, start a coroutine that waits for it to appear
				StartCoroutine(WaitForGameDataLoader());
			}
		}


		private System.Collections.IEnumerator WaitForGameDataLoader()
		{
			while (GameDataLoader.instance == null)
			{
				yield return null;
			}
			// now instance exists; try to use it
			if (GameDataLoader.instance.loadedConfig != null)
			{
				Ip = GameDataLoader.instance.loadedConfig.serverIP;
				Port = GameDataLoader.instance.loadedConfig.serverPort;
			}
			else
			{
				GameDataLoader.instance.OnGameDataUpdated += OnGameDataUpdated;
				StartCoroutine(WaitForConfig());
			}
		}

		private System.Collections.IEnumerator WaitForConfig()
		{
			// wait a few frames for loadedConfig to be available
			int maxFrames = 300; // ~5 seconds at 60fps
			int frames = 0;
			while ((GameDataLoader.instance == null || GameDataLoader.instance.loadedConfig == null) && frames++ < maxFrames)
			{
				yield return null;
			}
			if (GameDataLoader.instance != null && GameDataLoader.instance.loadedConfig != null)
			{
				Ip = GameDataLoader.instance.loadedConfig.serverIP;
				Port = GameDataLoader.instance.loadedConfig.serverPort;
				// unsubscribe if we had subscribed earlier
				if (GameDataLoader.instance != null)
					GameDataLoader.instance.OnGameDataUpdated -= OnGameDataUpdated;
			}
			else
			{
				Debug.LogWarning("ServerComunication: GameDataLoader.loadedConfig not available after waiting.");
			}
		}

		private void OnGameDataUpdated()
		{
			if (GameDataLoader.instance != null && GameDataLoader.instance.loadedConfig != null)
			{
				Ip = GameDataLoader.instance.loadedConfig.serverIP;
				Port = GameDataLoader.instance.loadedConfig.serverPort;
				GameDataLoader.instance.OnGameDataUpdated -= OnGameDataUpdated;
			}
		}

		private string GetFullEndGameUrl(string nfcId)
		{
			return $"http://{Ip}:{Port}/users/{nfcId}/endgame";
		}

		private string GetFullNfcUrl(string nfcId)
		{
			return $"http://{Ip}:{Port}/users/{nfcId}";
		}

		/// <summary>
		/// Informação deve ser enviada após o jogo para atualizar a pontuação do jogador, aconselho colocar o id do jogo por Json
		/// </summary>
		/// <param name=">gameInfo"><see cref="GameModel"/> Pontuação de cada jogo funciona diferente, olhar no <see href="https://docs.google.com/document/d/14COKL4PcHkT3_J_TiCc79gAZNwbT6pKFmMdV3G9mY0Q/edit?usp=sharing">documento</see></param>
		/// <returns>Codigo de status do update ao server <see cref="HttpStatusCode"/></returns>
		public async Task<HttpStatusCode> UpdateNfcInfoFromGame(GameModel gameInfo)
		{
			var url = GetFullNfcUrl(gameInfo.nfcId);
			var request = new HttpRequestMessage(HttpMethod.Post, url);
			var content = new StringContent(gameInfo.ToString());
			request.Content = content; 
			Debug.Log($"Sending to {url}");
			var response = await _client.SendAsync(request);
			return response.StatusCode;
		}

		/// <summary>
		/// Usado para pegar as informações atuais do nfc
		/// </summary>
		/// <param name="nfcId">Nome enviado pelo <see cref="NFC.NFCReceiver"/></param>
		/// <returns><see cref="EndGameResponseModel"></see></returns>
		public async Task<EndGameResponseModel> GetNfcInfo(string nfcId)
		{
			var url = GetFullNfcUrl(nfcId);
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			Debug.Log($"Sending to {url}");
			var response = await _client.SendAsync(request);
			Debug.Log($"Response code is {response.StatusCode}");
			if (response.StatusCode == HttpStatusCode.OK)
			{
				var body = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<EndGameResponseModel>(body);
			}
			else
			{
				return null;
			}
		}
		
		/// <summary>
		/// Usado para enviar o nome do usuario para o Banco de dados e avisar que já passou pela última experiência
		/// </summary>
		/// <param name="endGameRequestModel"></param>
		/// <param name="nfcId"></param>
		/// <returns></returns>
		public async Task<EndGameResponseModel> PostNameForEndGameInfo(EndGameRequestModel endGameRequestModel, 
			string nfcId)
		{
			var url = GetFullEndGameUrl(nfcId);
			var request = new HttpRequestMessage(HttpMethod.Post, url);
			var content = new StringContent(endGameRequestModel.ToString());
			request.Content = content;

			Debug.Log($"Sending to {url}");
			var response = await _client.SendAsync(request);
			Debug.Log($"Response code is {response.StatusCode}");
			if (response.StatusCode == HttpStatusCode.OK)
			{
				var body = await response.Content.ReadAsStringAsync();
				return Newtonsoft.Json.JsonConvert.DeserializeObject<EndGameResponseModel>(body);
			}
			else
			{
				Debug.Log($"Response code is {response.StatusCode}");
				return null;
			}
			
		}
		
	}
}