# ByBitSample
#### A simple market data provder for the ByBit crypto exchange

The project consists of two primary components: 
[ConsoleUI](https://github.com/insaneMonkeyz/ByBitSample/tree/main/ConsoleUI), 
which implements the console interface for user interaction, and 
[MarketDataProvider](https://github.com/insaneMonkeyz/ByBitSample/tree/main/MarketDataProvider), 
which encapsulates the communication logic with the exchange.

In the ConsoleUI project, we have a rendering class, 
[ConsoleUI](https://github.com/insaneMonkeyz/ByBitSample/blob/main/ConsoleUI/ConsoleUI.cs), 
and a 
[ConsoleViewModel](https://github.com/insaneMonkeyz/ByBitSample/blob/main/ConsoleUI/ConsoleViewmodel.cs)
class that handles communication between the view and application logic.

In the MarketDataProvider module, two primary interfaces facilitate interacting with the exchange - 
[IMarketDataProvider](https://github.com/insaneMonkeyz/ByBitSample/blob/main/MarketDataProvider/Public/IMarketDataProvider.cs) 
for managing market data and 
[IConnection](https://github.com/insaneMonkeyz/ByBitSample/blob/main/MarketDataProvider/Public/IConnection.cs) 
for managing connections.

The [IConnection](https://github.com/insaneMonkeyz/ByBitSample/blob/main/MarketDataProvider/Public/IConnection.cs) 
interface is implemented in the 
[WebSocketConnection](https://github.com/insaneMonkeyz/ByBitSample/blob/main/MarketDataProvider/WebSocket/WebSocketConnection.cs) 
class, which includes 
all the necessary connection support logic - automatic reconnection in case of interruptions 
and regular background server pings to prevent disconnection.

The [BybitMarketDataProvider](https://github.com/insaneMonkeyz/ByBitSample/blob/main/MarketDataProvider/BybitMarketDataProvider.cs) 
class implements the 
[IMarketDataProvider](https://github.com/insaneMonkeyz/ByBitSample/blob/main/MarketDataProvider/Public/IMarketDataProvider.cs) 
interface. It operates at a more abstract level, converting user requests into requests to the exchange.


### тоже самое, но другими буквами

Проект состоит из двух основных компонентов: 
[ConsoleUI](https://github.com/insaneMonkeyz/ByBitSample/tree/main/ConsoleUI),
воплощающий консольный интерфейс взаимодействия с пользователем, и 
[MarketDataProvider](https://github.com/insaneMonkeyz/ByBitSample/tree/main/MarketDataProvider), 
инкапсулирующий логику сообщения с биржей.

В проекте ConsoleUI есть 
[одноименный](https://github.com/insaneMonkeyz/ByBitSample/blob/main/ConsoleUI/ConsoleUI.cs) 
класс, занимающийся отрисовкой, и класс 
[ConsoleViewModel](https://github.com/insaneMonkeyz/ByBitSample/blob/main/ConsoleUI/ConsoleViewmodel.cs), 
реализующий взаимодействие представления с логикой приложения.

Для взаимодействия с биржей используются 2 основных интерфейса - 
[IMarketDataProvider](https://github.com/insaneMonkeyz/ByBitSample/blob/main/MarketDataProvider/Public/IMarketDataProvider.cs) 
для работы с данными и 
[IConnection](https://github.com/insaneMonkeyz/ByBitSample/blob/main/MarketDataProvider/Public/IConnection.cs) 
управления соединением.

Интерфейс [IConnection](https://github.com/insaneMonkeyz/ByBitSample/blob/main/MarketDataProvider/Public/IConnection.cs) 
реализуется классом 
[WebSocketConnection](https://github.com/insaneMonkeyz/ByBitSample/blob/main/MarketDataProvider/WebSocket/WebSocketConnection.cs). 
В нём вся логика по поддержке соединения - переподключение в случае разрывов и фоновом пинговании сервера, чтобы он сам не отключался.

Интерфейс 
[IMarketDataProvider](https://github.com/insaneMonkeyz/ByBitSample/blob/main/MarketDataProvider/Public/IMarketDataProvider.cs) 
реализуется классом 
[BybitMarketDataProvider](https://github.com/insaneMonkeyz/ByBitSample/blob/main/MarketDataProvider/BybitMarketDataProvider.cs), 
который работает на более высоком уровне абстракции, и занимается преобразованием запросов пользователя в запросы бирже.
