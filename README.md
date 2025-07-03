# Inspection System
An interactive system for inspecting in-game objects with ease. It enables immersive interaction with objects through selection, rotation, and zooming, making it ideal for slow-paced games like horror titles.

---

## Features
- Plug-and-play setup
- Click-to-examine functionality
- Smooth zoom and rotation controls
- Displays item name and description during inspection

---

## Getting Started
1. Attach the `InspectionSystem` script to your Main Camera or any GameObject under the Main Camera.  
2. Create a new empty child GameObject under the Main Camera and name it `InspectPoint`.  
3. Assign any GameObject you want to examine to the **Interactable** layer.  
4. Add a script that inherits from the abstract class `Items` to each inspectable GameObject:  
   - This script will hold a ScriptableObject reference containing the item’s name and description.  
   - **To create a new ScriptableObject data asset**, right-click in the Project panel, then choose `Create > Item Data` and fill in the variable fields.

Your hierarchy should look like this:   
Main Camera
└── InspectSystem (with InspectSystem script attached)
└── InspectPoint (empty GameObject)

---

## Code Structure
To view the codes, please nagivate to:

Assets/Inspect System/Scripts
