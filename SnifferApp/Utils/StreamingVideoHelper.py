import threading
import time
import cv2
import io
import PIL.Image as Image
import numpy as np
from Utils import Events
from PySide6 import QtCore


class StreamingVideoHelper(QtCore.QObject):
    qRenderingStartSignal = QtCore.Signal(int, str)
    qFrameRenderedSignal = QtCore.Signal(int)
    qRenderingEndSignal = QtCore.Signal()

    def __init__(self):
        super().__init__()
        self.allFrames = []
        self.startRecordingTime = 0
        self.endRecordingTime = 0
        self.fileSavedEvent = Events.Event()
        self.fileName: str = ""
        self.settingsFps = 12

    def addFrame(self, frame, seconds: int):
        self.allFrames.append(frame)

    def onRecordingStarted(self):
        self.startRecordingTime = time.time()

    def onRecordingEnded(self):
        self.endRecordingTime = time.time()

    def saveFramesToVideo(self, fileName: str):
        self.fileName = fileName
        saveTask = threading.Thread(target=self._saveFramesToVideo)
        saveTask.start()

    def _saveFramesToVideo(self):
        if len(self.allFrames) <= 0:
            return
        self.qRenderingStartSignal.emit(len(self.allFrames), "Rendering video...")

        image = Image.open(io.BytesIO(self.allFrames[0]))
        frameSize = (image.width, image.height)
        seconds = self.endRecordingTime - self.startRecordingTime
        desiredFrames = seconds * self.settingsFps
        avgSeconds = seconds / len(self.allFrames)

        while len(self.allFrames) < desiredFrames:
            self.duplicateFrames(desiredFrames)

        real_fps = len(self.allFrames) / seconds
        out = cv2.VideoWriter(self.fileName, cv2.VideoWriter_fourcc(*'MJPG'), real_fps, frameSize)

        for frame in self.allFrames:
            jpgAsNp = np.frombuffer(frame, dtype=np.uint8)
            img = cv2.imdecode(jpgAsNp, flags=1)
            out.write(img)
            self.qFrameRenderedSignal.emit(1)

        out.release()
        self.allFrames.clear()
        self.fileSavedEvent(fileName=self.fileName)
        self.qRenderingEndSignal.emit()

    def duplicateFrames(self, desiredFrames: int):
        tempFrames = []
        for i in range(len(self.allFrames)):
            frames = self.allFrames[i]
            if i % 2 == 0 and len(tempFrames) < desiredFrames:
                tempFrames.append(frames)
                tempFrames.append(frames)
            else:
                tempFrames.append(frames)

        self.allFrames = tempFrames
