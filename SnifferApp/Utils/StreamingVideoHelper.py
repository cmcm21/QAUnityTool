import cv2


class StreamingVideoHelper:
    def __init__(self):
        self.allFrames = []

    def restart(self):
        self.allFrames.clear()

    def addFrame(self, frame):
        self.allFrames.append(frame)

    def saveFramesToVideo(self, fileName: str):
        out = cv2.VideoWriter(fileName, cv2.VideoWriter_fourcc(*'DIVX'))

        for frame in self.allFrames:
            out.write(frame)

        out.release()
