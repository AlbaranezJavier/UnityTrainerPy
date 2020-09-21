# Real-time communication between Unity and Python for AI
With this repository you can establish a real time communication between Unity and your AI algorithms made with Python, such as training an autonomous car.

The available scenes consist of a series of tests from the Formula Student competition, which I will improve over time and incorporate the rest.

<p align="center">
  <img src="docs/video.gif" alt="video explanation gif" width="100%" />
</p>

## :selfie:Credit
If you find this repository useful or interesting, please give me credit.
- [LinkedIn](https://www.linkedin.com/in/javier-albar%C3%A1%C3%B1ez-mart%C3%ADnez-8b0b251b3/)
- [Github](https://github.com/AlbaranezJavier)

Excellent asset of a free car at the Unity store:
- [Ruslan, 3D Low Poly Car For Games (Tocus)](https://assetstore.unity.com/packages/3d/vehicles/land/3d-low-poly-car-for-games-tocus-101652)

## :bulb:Motivation
We are involved in the Formula Student competition, where the goal is to complete a series of tests with an autonomous car. But before deploying experiments in the real world, it is interesting to try and test them in a simulated environment.

## :checkered_flag:Objectives
* Allow communication between python and unity in real time, without having to adapt code.
* Perform stereo or mono camera simulations.
* Record the simulated sessions in images.
* Be adaptable to other types of simulations.

## :factory:Design
While with Unity the environment is simulated, with Python the decisions of the agent are taken (being in this case the car). Communication is established with "sockets", where Unity is the server side and Python is the client side.

### Requirements
* Unity 2019.4.3f1
* Python 3.x

### The "Server.cs" script (Unity):
This is responsible for managing communications between the environment and the agent. Establishing the minimum structure required (ServerUser) by the agent's controller in the environment.

### The "AIManager.cs" script (Unity):
This script controls and manages the vehicle's information and actuators. And it inherits the minimum structure needed to receive the information from the Python side (ServerUser).

### The "CarController.cs" script (Unity):
It manages the physics of the vehicle and its visual behaviour, as well as giving access to the telemetry of the vehicle.

### The "my_client.py" file (Python):
Establishes and manages communication with the server available in the ip of the computer where it runs, by default in the port "12345".

### The "test.py" file (Python):
It proposes an example structure to manage the receiver data, process an action and send it to the agent in the environment.

### The "recorder.py" file (Python):
Script dedicated to the recording of images received by the server. It follows the same structure as "test.py", so the data received from the environment could be easily stored in a CSV.

## :bookmark_tabs:Guides

### Workflow
1. Get the IPv4 address.
2. In Unity "UnityTrainerPy/LWRP UMotorsport", choose one of the scenes "Assets/Scenes/Simulator".
3. In the GameObject "Server", enter the IPv4 address in the Ip_address field of the "Server" script.
4. Activate the option "Stats" in the Game window, to see the resolution at which it is, because this will be its output size to python and resize it to the desired value.
5. Run Unity, where the vehicle controls are "w-accelerate", "s-reverse", "a-left turn", "d-right turn" and "space-brake".
6. Select the GameObject "Server" in Hierarchy, then select "Raise Server" in the "Server" script. This option will be highlighted in blue.
7. In Python "UnityTrainerPy/PyUMotorsport", run the "test.py" file.
8. If everything is correct, in the python console will start to appear the values collected in Unity (Speed, steering wheel rotation, accelerator, brake, etc). And python will be sending a "go" signal, that will appear in the "Received Data" field of the "AI Manager" script of the "Server" GameObject.

### Where does the agent receive information, process and send the decision?
On the Python side you can find a file called "test.py". This file contains a structure as an example that is divided into:
* Socket connection.
* Loop to send and receive messages
  * Obtaining the data sent by the environment. (image, accelerator, speed, brake, steering wheel, etc) 
  * Painting of the received image (optional action).
  * Sending of the action taken by the agent <-- point where the desired algorithm would be introduced.
  
### To record what happens in Unity in jpg images?
There is a tool available in the "PyUMotorsport/myTools" folder called "recorder.py", where you can set a saving directory and whether the recording will be with stereo image or not. Simply follow the process described in Workflow, but instead of executing "test.py" you execute "recorder.py".

Recorder and test follow the same structure, so at the time of the recording you have information of the environment, and can be saved in a CSV to have recorded paths with which to perform tests, understand or compare better the training process performed.

## :raised_hands:References
 - [Research group: CAPO](http://caporesearch.es/)
 - [Competition group: UMotorsport](http://u-motorsport.com/2019/08/19/umotorsport/)
 - [Competition: Formula Student](https://www.formulastudent.es/)
