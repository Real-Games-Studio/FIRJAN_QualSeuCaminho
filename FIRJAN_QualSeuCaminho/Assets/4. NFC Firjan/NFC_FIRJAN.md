Documentação de api: Firjan
Requerimentos:
Api Http Rest local com database
Json schemas:
NfcObject:
	String NfcId,
	List GameObject PlayedGames (Can be null),
	List AblityObject Abilities (Can be null)

AbilityObject:
	Int AbilityId,
	Int Points

GameObject:
	Int GameId,
	List AbilityObject Abilities


Get -
 /users/[NfcId] - Retorna o json do NfcObject desse NfcId no body - Erro 404 se não encontrado
/users - Retorna a quantidade de NfcObject que tem na database
Post -
/users/[NfcId] - Confirma se já existe esse NfcObject na DB se não tiver cria um novo - Erro 403 se já tiver esse NfcObject
Put - 
/users/[NfcId] {GameObject} - Confere se esse NfcId já existe e depois confere se esse GameObject.GameId já existe no NfcObject. Se o nfcId não existe cria um novo NfcObject e adiciona o GameObject, se o NfcId já existe olha se ele já tem o GameObject.GameId, se já existe troca os valores do GameObject.Abilities pro novo GameObject, se não existe adicionar para lista.


NfcId - vários cards NFC
GameId dos jogos
1	qual o caminho
2	fato ou fake
3	jogo dos 7 erros
4	Palavra chave
5	jogo da empatia
6	quem sabe ouvir
7	missão em equipe
8	mente aberta
9	industria da reciclagem
10	dilema do bonde
11	na raiz da questão
12	letramento digital
13	Certificado de Habilidades

AbilityId (30)
8 jogos com 3 habilidades - 24 habilidades - Liderança = Ha01 + Ha23
serverconfig.JSON 

Ha01
Ha02
…

Mensagem post: JSON
{
“NfcId” : “nfc-123-test”,
“GameID” : 1,  
Ha01 : 9,
Ha03 : 6,
Ha22 :8 
}
- assim manter último score

DB local: PC servidor dentro do circuito

DB dashboard:

NfcId (Ha01,Ha02,....Ha30) (starttime) 

Mensagem get: NfcId + nome

Return: (Liderança(X), Comunicação(Y),Empatia…)
{
	NfcId : “123-test”,
	Nome : “jose”,
	Liderança : 1,
	Comunicação : 2,
	…
}

Log final do CardID+unixtime+nome+score_habilidades

Start Time = 0 e zera o cartão on acknowledge


Jogos e Habilidades - máximos

Jogo 1 - Qual é o caminho? 
Pensamento crítico - 9
Tomada de decisão - 9
Solução de problemas - 7

Jogo 03 - JOGO DOS 7 ERROS:
Empatia - 8 	
Criatividade - 7  	    
Resolução de problemas - 6

Jogo 10 - Dilema do Bonde
Raciocínio Lógico - 9        
Autoconsciência - 6         
Tomada de decisão - 6

Jogo 02 - Verdade ou Mito
Letramento Digital - 8 	
Pensamento analítico - 7  
Curiosidade - 6

Jogo 06 - Quem sabe ouvir?
Liderança - 8
Comunicação - 9  
Agilidade - 7

Jogo 12 - Letramento Digital
Letramento Digital - 8 
Curiosidade - 7 
Aprendizado Contínuo - 6 

Jogo 04 - Palavra-chave
Aprendizado contínuo - 8  
Adaptabilidade - 7  
Resiliência - 6

Jogo 05 - Jogo da Empatia - 
Empatia - 9 
Escuta Ativa - 7 
Autoconsciência - 5 


Totais possíveis:

Empatia	17
Letramento Digital	16
Tomada de decisão	15
Aprendizado contínuo	14
Curiosidade	13
Resolução de problemas	13
Autoconsciência	11
Comunicação	9
Pensamento crítico	9
Raciocínio Lógico	9
Liderança	8
Adaptabilidade	7
Agilidade	7
Criatividade	7
Escuta Ativa	7
Pensamento analítico	7
Resiliência	6

Resultados - como somar as habilidades:

Adaptabilidade (Jogo4.Ha02)
Agilidade (Jogo6.Ha03)
Aprendizado contínuo (Jogo4.Ha01, Jogo12.Ha03)
Autoconsciência (Jogo10.Ha02, Jogo5.Ha03)
Comunicação (Jogo6.Ha02)
Criatividade (Jogo3.Ha02)
Curiosidade (Jogo2.Ha03, Jogo12.Ha02)
Empatia (Jogo3.Ha01, Jogo5.Ha01)
Escuta Ativa (Jogo5.Ha02)
Letramento Digital (Jogo2.Ha01, Jogo12.Ha01)
Liderança (Jogo6.Ha01)
Pensamento analítico (Jogo2.Ha02)
Pensamento crítico (Jogo1.Ha01)
Raciocínio Lógico (Jogo10.Ha01)
Resiliência (Jogo4.Ha03)
Resolução de problemas (Jogo1.Ha03, Jogo3.Ha03)
Tomada de decisão (Jogo1.Ha02, Jogo10.Ha03)

Payload POST:
{
  "nfcId": "CARD123",
  "gameId": 1,
  "skill1": 3,
  "skill2": 2,
  "skill3": 4
}

Retorno:
{
  "nfcId": "CARD123",
  "status": "active",
  "timer": "2025-09-16T18:30:00Z",
  "games": {
    "1": { "skill1": 3, "skill2": 2, "skill3": 4 }, 
     "2": { "skill1": 3, "skill2": 2, "skill3": 4 }, …
  },
  "attributes": {
    "critical_thinking": 3.0,
    "decision_making": 2.0,
    "problem_solving": 4.0,
    "adaptability": 0.0,
    "agility": 0.0,
    "...": 0.0
  }
}



Pacote Unity:
Link: unity.package

Leitor de NFC:


On NFC Connected envia 2 strings aonde a primeira é o id do nfc e a segunda é o nome do leitor.

On NFC Disconnected chama quando o NFC é disconectado do leitor

On NFC Reader Connected envia 1 string com o nome do leitor conectado.


On NFC Reader Disconnected chama quando o NFC Reader é desconectado do pc.
Comunicação com o Server


IP e Port devem ser colocados no Json.

Dentro tem 3 Tasks diferentes:


UpdateNfcInfoFromGame: envia pro server um post atualizando as informações do NFC usando o GameModel.

GetNfcInfo: pede pro server as informações do NfcId enviado.

PostNameForEndGameInfo: envia pro server um nome pra ser colocado junto com o nfcId.

