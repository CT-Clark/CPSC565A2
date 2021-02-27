# CPSC565A2
This repo holds an implementation of a simplified Quidditch simulation written in Unity for assignment 2 of CPSC565 - Emergent Computing - during the Winter semester of 2021. 

Disclaimer:
Some of the code implemented is from/based on Unity's TANKS! tutorial which can be found here: https://www.youtube.com/watch?v=paLLfWd2k5A&list=PLclF8feY8EKLw5Un6Z2Syt2_35qsdpnHt 
https://github.com/omaddam/Boids-Simulation has also lent a sizeable amount of code and code ideas to this progect. Any code that looks similar between this project and those sources is likely because it is, and I take no credit. This project is not intended to be used for commercial purposes. 

-----------------------------
// ----- THE PROJECT ----- //
-----------------------------

From the assignment PDF: Quidditch is a competitive sport in the wizarding world of Harry Potter, a series of fantasy novels written by J. K. Rowling. It is a game played by wizards and witches, where there’re two teams riding on flying broomsticks trying to score as many points as possible. In this assignment we will implement a simplified version of this game. Our simplified game of quidditch is played with two teams. The game involves players flying around a stadium and attempting to score 1 point by catching the golden snitch. Successive catches are worth 2 points each so it is best not to let your opponents get a streak going. The first team to 100 points wins. 

This project aims to bring about emergents through the playing of quidditch. Each agent player within the game must make decisions about where to go based on a number of rules. The rules are as follows:
  1) The goal of the game is to catch the golden snitch, therefore the agent is extremely attracted towards the snitch.
  2) Agents who collide with structure in the game world are automatically rendered unconscious, therefore they will attempt to prevent any such collisions.
  3) Agents who collide with other agents have a chance to be "tackled" and knocked unconscious, therefore they try to maintain a bit of distance from other players.

This project placed the following requirements upon the players:
  1) Each Quidditch player will be modeled by an object that can indicate its direction and is colour coded by team (red for Gryffindor and green for Slytherin).
  2) Players and snitch ignore gravity while flying. 
  3) However, you must properly implement momentum (heavier objects accelerate slower).
  4) The snitch must be modelled by a golden sphere which can also indicate direction.
  5) The players and snitch must be constrained within a suitably sized arena. No flying outside the bounds of the arena, and players colliding with the terrain will be considered “tackled” (see further requirements).
  6) The snitch’s movement must be smoothly random (no jerky randomness).
  7) Player behavior needs to be driven by forces. A force must exist which attracts the player toward the snitch, and a force must exist for players to not collide with each    other or the environment. 
  8) Players have specific traits. Aggressiveness, max exhaustion, max velocity, weight, and current exhaustion. A Players velocity can never exceed their max Velocity.
  9) As players move, their current exhaustion increases When current exhaustion is equal to max exhaustion a player is considered “unconscious.” 
  10) When two players collide, a calculation must be run to determine which player becomes “unconscious”.  The calculation is as follows: (Player1Value = player1.Aggressiveness * (rng.NextDouble() * (1.2 –0.8) + 0.8) *  (1 –(player1.exhaustion / player1.maxExhaustion))Player2Value = player2.Aggressiveness * (rng.NextDouble() * (1.2 –0.8) + 0.8) * (1 –(player2.exhaustion / player2.maxExhaustion))The player with the lower value is the one to become “unconscious”. In the event of the collision being between two players on the same team only 5% of the time should it result in an “unconscious” player. 
  11) Unconscious players ignore all forces except gravity. 
  12) Upon colliding with the ground, an unconscious player is teleported back to its team’s starting position, put on hold for a variable number frames, and no longer considered unconscious.

This project placed the following requirements upon the teams:
  1) There should be at least 5 players in each team and at most 20, and that each team should have its own single starting point on the ground.
  2) The traits you assign to your teams must match the following values:
  3) Slytherin Trait Defaults: Weight: x̄= 85, σ= 17 Max Velocity: x̄= 16, σ= 2 Aggressiveness: x̄= 30, σ= 7 Max Exhaustion: x̄= 50, σ= 15
  4) Gryffindor Trait Defaults: Weight: x̄= 75, σ= 12 Max Velocity: x̄= 18, σ= 2 Aggressiveness: x̄= 22, σ= 3 Max Exhaustion: x̄= 65, σ= 13 
  5) Traits were implemented with a Box-Muller transform to fit a specific distribution.

This project also included the following behavior requirements:
  1) You must further come up with two more traits per team and incorporate them into the behavior of the players somehow.
  2) You must implement some sort of behavior which inhibits players going unconscious from exhaustion. The simplest form of this will be having players stop moving to “recharge” their exhaustion.

--------------------------------
// ----- IMPLEMENTATION ----- //
--------------------------------

I will begin at the bottom of the listed requirements mentioned above.

// ----- Exhaustion and Exhaustion Inhibition ----- //
Exhaustion, contrary to what was recommended in the assignment description, was implemented on a per-physics-frame basis. Each time the player performs the Move() method their exhaustion is increased by 0.01. This gives them enough time before needing to take a break to accomplish their goals. Although the unconsciousness from exhaustion has been implemented, the agents are instructed to stop all movement and enter a recovering state if thier current exhaustion is one unit less than their maximum exhaustion. While they're recovering their reduce their exhaustion levels by 0.1, 10x as fast as their exhaustion depletes. This continues until their current exhaustion level is half of their maximum exhaustion level. The other time exhaustion level changes is when they've fallen unconscious, have hit the ground, and are respawning. I've considered this a 'rest' and reset their exhaustion levels to 0. While they're resting their is the possibility of other agents crashing into them, but that's what you get for resting in the middle of the playing field!

// ----- Additional Implemented Traits ----- //
The two additional traits I've implemented to play around with the agents' max velocity trait. The first ability is an 'underdog' ability. Whenever the other team is up by 3+ points then all players on your team gain an increase to their maximum velocity by a value generated on instantiation. This allows members on those team to fly quicker to the new snitch location and have a chance at catching it before their opponents get there. There is a downside though, as this increase in max velocity comes at the price of an increased rate of exhaustion. 
The second trait I've implemented is a 'captain' trait. The first agent created for each team is designated the 'captain', and any agents less than their captain influence radius away from the team's captain recieves a bonus to their max velocity. The captain, however, does not recieve this bonus. This leads to the captain inspiring their teammates to go forth quicker to catch the snitch before the enemy is able to. 
Both of these traits als ouse the BoxMuller transform to generate unique random trait levels from a normal distribution upon agent instantiation. Gryffindor players have a generally higher and wider breadth of an underdog bonus (no one wants Slytherins to win, even if they are the underdogs). Gryffindor players recieve a higher bonus from being nearer to their captain, but need to be nearer to their captain than Slytherins in order to take advantage of this fact. 

// ----- Camera Implementation ----- //
In order to use the gamera during play the QWEASD and arrow keys will be used. The arrow keys control rotation of the camera (Press down to look down, right to look right), the WASD keys are used to move forward/backwards/pan left/pan right respectively, while the QE keys are used to move downwards/upwards respectively. Zoom is not enabled. Technically you can position the camera wherever you want with just the WASD and arrow keys, but the QE keys help make this action just a little bit simpler. 

// ----- Respawn Implementation ----- // 
Respawning is implemented by generation a random number between 0 and 1000 inclusive, and if it's below 10 then the agent is free to resume the game. This gives a variable number of frames which the agent must wait through. 

// ----- Snitch Implementation ----- //
The snitch flies randomly by picking a random point within the arena and attempting to fly towards it. This ends up building up enough momentum that the snitch often flies back and forth from one side to the other. Because the snitch is bound by artificially set limits, the agents in their quest to catch the snitch are also soft-bounded within this environment as well. Once the snitch is 'caught' (An agents collides with it) then its location is changed to a random location within its possible range of locations. The snitch also has a light source on it fo a little bit better tracking. 

----------------------------------
// ----- EXAMPLE GAMEPLAY ----- //
----------------------------------
![Record1](https://user-images.githubusercontent.com/23039052/109377316-c66ecb80-7887-11eb-9ce7-626acfbb438e.gif)
In this image we see an example of typical gameplay. The direction of the agents are indicated with arrows protruding from their models, and the golden snitch's location and direction is indicated by a yellow arrow protruding from it. (Not pointing to it) We see that when players collide they tumble to the ground until they hit it, and then they reappear near their team's 'respawn zone'. The exact location is randomized upon their instantiation so that all players on a team start from a very slightly different location. This example was produced without the ground/scenery collision forces, as well as without the forces which encourage agents to stay further apart from each other. 

![Record2](https://user-images.githubusercontent.com/23039052/109377386-51e85c80-7888-11eb-996f-0ce315a3ae25.gif)
In this example we see what happens when the ground collision force and the force which attracts them to the snitch are turned off. They're all repulsed away from one another and end up spreading out. 

![Record3](https://user-images.githubusercontent.com/23039052/109377458-c91df080-7888-11eb-8f0d-c9e1f5b99f66.gif)
This is an example of the fully featured game in play. I've artificially boosted Gryffindor's points count in order to demonstrate the effect of the underdog mechanic. We can see Slytherin's agents have a slightly higher top speed, enabling them to chase down the snitch a little quicker. We also see the score keeping UI implemented. Once a team accumulates 100 points a "Team WINS THE GAME" statement replaces the usual scores as shown below.

![Screenshot 2021-02-26 232004](https://user-images.githubusercontent.com/23039052/109377516-30d43b80-7889-11eb-8504-c5fdb8884b4b.png)

We also see that the camera is capable of moving within the same game to a new vantage point![Screenshot 2021-02-26 232402](https://user-images.githubusercontent.com/23039052/109377652-c96abb80-7889-11eb-86b3-1b5920996cb1.png)
.
