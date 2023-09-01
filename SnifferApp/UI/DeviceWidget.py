from PySide6 import QtWidgets, QtCore
from Network.Clients.DeviceClient import DeviceClient, DeviceState
from PySide6.QtGui import QPixmap
from UI.UIVideoPlayer import VideoPlayer


class DeviceWidget(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        self._initButtons()
        self.streamingSize = QtCore.QSize(800, 400)
        self.deviceState = DeviceState.IDLE
        self.noStreamingPath = "Assets/NoStreamingImg.png"

        self.container = QtWidgets.QHBoxLayout(self)
        self._setStreamingLayout()
        self._setButtonsLayout()

        self.container.addLayout(self.buttonsLayout)
        self.container.addLayout(self.streamingContainer)

    def setStreamingImage(self, pixmap: QPixmap):
        if self.deviceState == DeviceState.IDLE:
            return

        pixmap = pixmap.scaled(self.streamingSize, aspectMode=QtCore.Qt.AspectRatioMode.KeepAspectRatio)
        self.streamingLabel.setPixmap(pixmap)

    def _setStreamingLayout(self):
        self.streamingContainer = QtWidgets.QVBoxLayout(self)

        self.noStreamingPixmap = QPixmap(self.noStreamingPath)
        #self.noStreamingPixmap.scaled(self.streamingSize, QtCore.Qt.AspectRatioMode.KeepAspectRatio)

        self.streamingLabel = QtWidgets.QLabel("Streaming Video")
        self.streamingLabel.setPixmap(self.noStreamingPixmap)

        self.streamingContainer.addWidget(self.streamingLabel)

    def _initButtons(self):
        self.recordBtn = QtWidgets.QPushButton("Record")
        self.stopBtn = QtWidgets.QPushButton("Stop")
        self.replayBtn = QtWidgets.QPushButton("Replay")
        self.stopReplayBtn = QtWidgets.QPushButton("Stop Replay")
        self.loadFileBtn = QtWidgets.QPushButton("Load")

        self._setDefaultButtons()

        self.recordBtn.clicked.connect(self.onRecordBtnClicked)
        self.stopBtn.clicked.connect(self.onStopBtnClicked)
        self.loadFileBtn.clicked.connect(self.onLoadBtnClicked)
        self.replayBtn.clicked.connect(self.onReplayBtnClicked)

    def setState(self, state: DeviceState):
        self.deviceState = state
        if self.deviceState == DeviceState.IDLE:
            self.resetStreaming()

    def _setDefaultButtons(self):
        self.stopBtn.setEnabled(False)
        self.replayBtn.setEnabled(False)
        self.stopReplayBtn.setEnabled(False)

    @QtCore.Slot()
    def onRecordBtnClicked(self):
        self.stopBtn.setEnabled(True)
        self.loadFileBtn.setEnabled(False)
        self.replayBtn.setEnabled(False)
        self.stopReplayBtn.setEnabled(False)

    @QtCore.Slot()
    def onStopBtnClicked(self):
        self.replayBtn.setEnabled(True)
        self.loadFileBtn.setEnabled(True)

    @QtCore.Slot()
    def onLoadBtnClicked(self):
        self.replayBtn.setEnabled(True)

    @QtCore.Slot()
    def onReplayBtnClicked(self):
        self.stopReplayBtn.setEnabled(True)

    def _setButtonsLayout(self):
        self.buttonsLayout = QtWidgets.QVBoxLayout(self.container.widget())
        self.buttonsLayout.addWidget(self.recordBtn)
        self.buttonsLayout.addWidget(self.stopBtn)
        self.buttonsLayout.addWidget(self.loadFileBtn)
        self.buttonsLayout.addWidget(self.replayBtn)
        self.buttonsLayout.addWidget(self.stopReplayBtn)

    def resetStreaming(self):
        self.streamingLabel.setPixmap(self.noStreamingPixmap)

    def noDevices(self):
        self.hide()
        self.resetStreaming()

    def deviceSelected(self, device: DeviceClient):
        self._setDefaultButtons()
        self.show()

    def __del__(self):
        return
