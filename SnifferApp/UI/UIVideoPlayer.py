from PySide6 import QtWidgets, QtMultimediaWidgets, QtMultimedia, QtCore
from Utils.Events import Event


class VideoPlayer(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        self.player = QtMultimedia.QMediaPlayer()
        self.videoWidget = QtMultimediaWidgets.QVideoWidget(self)
        self.player.setVideoOutput(self.videoWidget)
        self.slider = QtWidgets.QSlider(QtCore.Qt.Orientation.Horizontal)
        self.vBox = QtWidgets.QVBoxLayout(self)
        self.onPlayerFinished = Event()
        self.player.positionChanged.connect(self._onPlayerPositionChanged)
        self.slider.sliderMoved.connect(self._onPositionChanged)

        self.vBox.addWidget(self.videoWidget)
        self.vBox.addWidget(self.slider)

    def _onPlayerPositionChanged(self, value: int):
        self.slider.setValue(value)
        if self.player.position() >= self.player.duration():
            self.onPlayerFinished()

    def _onPositionChanged(self, value: int):
        self.player.setPosition(value)

    def setSize(self, size: QtCore.QSize):
        self.videoWidget.resize(size)

    def loadVideo(self, videoPath: str):
        self.player.setSource(QtCore.QUrl().fromLocalFile(videoPath))
        self.slider.setMaximum(self.player.duration())

    def play(self):
        self.player.play()

    def stop(self):
        self.player.stop()

