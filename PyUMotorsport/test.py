import time, os, cv2
import numpy as np
from PIL import Image
from myTools.my_client import bind2server

if __name__ == '__main__':
    # Link is established with the server
    mySocket = bind2server()
    msg_size, channels, parameters_size = 8, 3, 29

    # Message format [imageWidth, imageHeigth, numberofCameras, decimalAccuracy, throttle, speed, steer, brake, image]
    index = [4, 8, 12, 16, 20, 24, 28, 29]

    # Image processing
    try:
        mySocket.sendall("ready".encode())
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
            throttle = int.from_bytes(msg[index[3]: index[4]], byteorder='little', signed=True)/decimalAccuracy
            speed = int.from_bytes(msg[index[4]:index[5]], byteorder='little', signed=False)
            steer = int.from_bytes(msg[index[5]:index[6]], byteorder='little', signed=True)/decimalAccuracy
            brake = int.from_bytes(msg[index[6]:index[7]], byteorder='little', signed=False)

            imageSize = (imageWidth * imageHeigth * 3) // numberOfCameras
            print(f'imageWidth: {imageWidth}, imageHeigth: {imageHeigth}, imageSize: {imageSize}, numberOfCameras: {numberOfCameras}')
            print(f'decimalAccuracy: {decimalAccuracy}, throttle: {throttle}, speed: {speed}, steer: {steer}, brake: {brake}')

            # Paint images
            image = []
            _index = index[7]
            for i in range(numberOfCameras):
                image.append(Image.frombytes("RGB", (imageWidth, imageHeigth//numberOfCameras), msg[_index:imageSize + _index]).transpose(
                    method=Image.FLIP_TOP_BOTTOM))
                cv2.imshow('output'+str(i), cv2.cvtColor(np.array(image[i]), cv2.COLOR_RGB2BGR))
                _index += imageSize

            # Sending the message
            mySocket.sendall("go".encode()) # <- Send a correct action.
            if cv2.waitKey(1) == ord('q'):
                break
        mySocket.close()
    finally:
        mySocket.close()

