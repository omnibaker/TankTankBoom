# TankTankBoom

Unity artillery game based on classics like *Scorched Earth* and *Death Tank*. It's a demo project displaying uses of Unity for 2D game development.

You control the trajectory of artillery fire against a rival tank. First to destory the other wins the battle. You will also have to fight off bomb-dropping blimps and berserker car bombers.

The terrain is generated randomly from various procedural methods. They are rendered using Unity's TileMap system to create a block grid of tile elements which can be destroyed. The UI is created with UIToolkit using various bindings, templates and custom elements.

<img width="400" height="185" alt="ttb_1" src="https://github.com/user-attachments/assets/1a93bb00-7db4-4efc-86e4-3fa655693673" background-color=#F00>

<img width="400" height="185" alt="ttb_11" src="https://github.com/user-attachments/assets/d37db351-84b6-484b-9870-74aef32a283b" />

<img width="400" height="185" alt="ttb_10" src="https://github.com/user-attachments/assets/eb549a00-a839-40d7-a0e5-0925a6ca3c74" />

<img width="400" height="185" alt="ttb_9" src="https://github.com/user-attachments/assets/768d898c-27d9-465c-9a52-1072583b0ca8" />

<img width="400" height="185" alt="ttb_5" src="https://github.com/user-attachments/assets/a944abbb-feed-4eb2-a096-e45b1f75b7d4" />

<img width="400" height="185" alt="ttb_6" src="https://github.com/user-attachments/assets/528af122-a777-4f25-81c8-65f4d1fe2611" />

<img width="400" height="185" alt="ttb_3" src="https://github.com/user-attachments/assets/6809ca13-9ed3-4370-adfc-c27615b555b0" />

<img width="400" height="185" alt="ttb_7" src="https://github.com/user-attachments/assets/fe527cb9-eed5-4170-928b-f90a4a32726b" />


## How to use
- Press START at main menu, the first battle will start immediately
- You are the green tank. Using the power and angle levels at the bottom-left, adjust your artillery trajectory.
- The firing guage will increment up between shots. When it displays "FIRE!" press and hold the target button on the right.
- While holding, an accuracy bar will appear and oscillate from left to right, indicating the accuracy offset - aim for the middle and release.
- Note the wind factor as you will need to adjust your accuracy to adapt to what way the wind is blowing.

Still a work in progress, however the core game is there. Audio set up but need to add sound effect asset. A few other loose ends to tie up but the dmeo is more of an example of a game with bootstrapper, menus, settings, full gameplay, consistent code, UI implementation,  and well-structured file manangement.

Simple C# with standard `UnityEngine` library. Works with native Unity 6.2. Feel free to use.

**Create by Stephen Baker, 2025**

_Check out my [Sumfulla Games](https://www.sumfulla.com/) website if you would like to support my games, this sort of content, or just general Unity shenanigans._
   
