import time
import cv2
import io
import PIL.Image as Image
import numpy as np
from Utils import Events


class StreamingVideoHelper:
    def __init__(self):
        self.allFrames = []
        self.startRecordingTime = 0
        self.endRecordingTime = 0
        self.fileSavedEvent = Events.Event()

    def restart(self):
        self.allFrames.clear()

    def addFrame(self, frame):
        self.allFrames.append(frame)

    def onRecordingStarted(self):
        self.startRecordingTime = time.time()

    def onRecordingEnded(self):
        self.endRecordingTime = time.time()

    def saveFramesToVideo(self, fileName: str):
        if len(self.allFrames) <= 0:
            return

        image = Image.open(io.BytesIO(self.allFrames[0]))
        frameSize = (image.width, image.height)
        seconds = self.endRecordingTime - self.startRecordingTime

        fps = len(self.allFrames) / seconds
        out = cv2.VideoWriter(fileName, cv2.VideoWriter_fourcc(*'MJPG'), fps, frameSize)

        for frame in self.allFrames:
            jpgAsNp = np.frombuffer(frame, dtype=np.uint8)
            img = cv2.imdecode(jpgAsNp, flags=1)
            out.write(img)

        out.release()
        self.fileSavedEvent(fileName=fileName)
