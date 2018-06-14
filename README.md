# MidNight introduction

MidNight helps you build your instance management, lobby and other management/metagame features of your game inside Unity alongside game servers using the same uNet which you already know. It does provide some of the things you need and hopefully will be added to it more and more.

## Features

- Accounts
- A good asynchronous model better than callbacks using promises (called Result in the framework)
- data storage abstraction layer to support multiple databases
- game instance management API with the ability to support multiple machines

## Known issues

- Does not have any built-in stateful horizontal scalability and you either need to rely on your database and stateless instances of your code or write it yourself
- Fault tolerance 

So can you use it? yes but for example if your games becomes big enough that one single lobby instance cannot support it then you have to add stuff to the codebase which can help you run multiple lobby instances and still have all friends features accessible because half of your players will connect to the first instance and half to the second.
About fault tolerance, it means you have to write your own monitoring process to restart the processes if they crash, Game instance crashes will be reported to you in the instance manager however because they'll be disconnected from the instance manager.

## How to use this and docs

Your application will consist of a lobby, a master of instance management and several instance management slaves which start your game instances on multiple machines. 
There are sample scenes and usecases inside the codebase and we'll be adding to it and the docs.

# Custom development and support

We provide custom development and support at [NoOpArmy](www.nooparmygames.com) for this. Also we have other products which help in the development of the multiplayer games and server backends.

# License

The MIT license. You are encouraged to use and extend the library and we provide custom development on it as well.
