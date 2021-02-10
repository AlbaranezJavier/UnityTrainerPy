import time, cv2
import numpy as np
from PIL import Image
from myTools.my_client import bind2server
import agent

'''
Main script, where messages are received from the server and agent actions are sent.
'''

if __name__ == '__main__':
    # Link is established with the server
    mySocket = bind2server()
    msg_size, channels, parameters_size = 8, 3, 29
    verbose = 1  # 0 = no output, 1 = only telemetry, 2 = all message and paint images

    # Message format [imageWidth, imageHeigth, numberofCameras, decimalAccuracy, throttle, speed, steer, brake, image]
    index = [4, 8, 12, 16, 20, 24, 28, 29]

    # Image processing
    try:
        mySocket.sendall("ready".encode())
        print("Established connection")
        while True:
            # Message reconstruction
            msg = mySocket.recv(msg_size)
            imageWidth = int.from_bytes(msg[:index[0]], byteorder='little', signed=False)
            imageHeigth = int.from_bytes(msg[index[0]:index[1]], byteorder='little', signed=False)
            sum = msg_size
            tam = imageWidth * imageHeigth * channels + parameters_size
            while sum < tam:
                data = mySocket.recv(tam)
                sum += len(data)
                msg = b"".join([msg, data])

            # Message content
            imageWidth = int.from_bytes(msg[:index[0]], byteorder='little', signed=False)
            imageHeigth = int.from_bytes(msg[index[0]:index[1]], byteorder='little', signed=False)
            numberOfCameras = int.from_bytes(msg[index[1]:index[2]], byteorder='little', signed=False)
            decimalAccuracy = int.from_bytes(msg[index[2]:index[3]], byteorder='little', signed=False)
            throttle = int.from_bytes(msg[index[3]: index[4]], byteorder='little', signed=True) / decimalAccuracy
            speed = int.from_bytes(msg[index[4]:index[5]], byteorder='little', signed=False)
            steer = int.from_bytes(msg[index[5]:index[6]], byteorder='little', signed=True) / decimalAccuracy
            brake = int.from_bytes(msg[index[6]:index[7]], byteorder='little', signed=False)

            images = []
            _index = index[7]
            imageSize = (imageWidth * imageHeigth * 3) // numberOfCameras
            for i in range(numberOfCameras):
                images.append(Image.frombytes("RGB", (imageWidth, imageHeigth // numberOfCameras),
                                             msg[_index:imageSize + _index]).transpose(method=Image.FLIP_TOP_BOTTOM))
                if verbose == 2:
                    cv2.imshow('output' + str(i), cv2.cvtColor(np.array(images[i]), cv2.COLOR_RGB2BGR))
                _index += imageSize

            if verbose == 1:
                print('\r', f'Number_cameras: {numberOfCameras}, throttle: {throttle}, brake: {brake}, steer: {steer}', end='')
            elif verbose == 2:
                print(
                    '\r', f'ImageWidth: {imageWidth}, imageHeigth: {imageHeigth}, imageSize: {imageSize}, number_cameras: {numberOfCameras}, decimalAccuracy: {decimalAccuracy}, throttle: {throttle}, brake: {brake}, steer: {steer}',
                    end='')
                for i in range(numberOfCameras):
                    cv2.imshow('output' + str(i), cv2.cvtColor(np.array(images[i]), cv2.COLOR_RGB2BGR))

            # Message to send
            # time.sleep(2)
            actions = agent.testAction(images, speed, throttle, steer, brake)
            msg = f'{actions[0]} {actions[1]} {actions[2]}'
            mySocket.sendall(msg.encode())  # <- Sends the agent's actions
            if cv2.waitKey(1) == ord('q'):
                break
        mySocket.close()
    finally:
        mySocket.close()
