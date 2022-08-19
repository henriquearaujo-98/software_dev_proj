## Software Development Project 
This project was developed within the course of “Software Development Project” offered by FHWS.

It consists on a multiplayer game developed in the Unity game engine. To that end, it was developed two sub-projects separated in two different Unity solutions. Namely a sub-project for the server side of the video game, and another sub-project for the client-side.

The way the game is going to operate over the network goes as follows:

1.	On the client project, there is a representation of the world that is simulated also on the server. In this sense, both projects are rendering the same world.
2.	The client sends player inputs to the server, including movement inputs, shooting, jumping and other mechanics.
3.	The server then takes those inputs, calculates a new game state and then sends that game state to the client so that they are both in sync.

Author: Henrique Araújo

