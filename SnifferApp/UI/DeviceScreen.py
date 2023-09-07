from PySide6 import QtWidgets, QtCore, QtGui

from Network.Clients.DeviceClient import DeviceClient, DeviceState
from UI.ProgressBar import ProgressBar
from Utils.StreamingVideoHelper import StreamingVideoHelper


class DeviceScreen(QtWidgets.QWidget):

    def __init__(self):
        super().__init__()
        self.device = None
        self.address = None
        self.size = QtCore.QSize(400, 75)
        self.noStreamingPath = "Assets/NoStreamingImg.png"
        self.qNameLabel = QtWidgets.QLabel(str(self.address))
        self.qStreamingLabel = QtWidgets.QLabel("Video Streaming")
        self.progressBar = ProgressBar(QtCore.QSize(435, 15))
        self.qvBox = QtWidgets.QVBoxLayout()
        self.qvBox.addWidget(self.qNameLabel, alignment=QtCore.Qt.AlignmentFlag.AlignLeading)
        self.qvBox.addWidget(self.qStreamingLabel, alignment=QtCore.Qt.AlignmentFlag.AlignLeading)
        self.qvBox.addWidget(self.progressBar)
        self.qvBox.setSpacing(1)
        self.qvBox.setContentsMargins(5, 5, 5, 5)
        self.progressBar.setVisible(False)

        self._showDefaultImage()
        self.setLayout(self.qvBox)

    def setDevice(self, device: DeviceClient):
        self.device = device
        self.address = device.address
        self.qNameLabel.setText(str(device.hostname))
        self.device.stateChangedEvent += self._onDeviceChangedState

    def setHelper(self, helper: StreamingVideoHelper):
        self.progressBar.setStreamingHelper(helper)

    def _onDeviceChangedState(self, *args, **kwargs):
        if 'state' not in kwargs:
            return

        state = kwargs['state']
        if state == DeviceState.IDLE:
            self._showDefaultImage()

    def reset(self):
        self._showDefaultImage()
        self.qNameLabel.setText("None")

    def _showDefaultImage(self):
        pixmap = QtGui.QPixmap(self.noStreamingPath)
        pixmap = pixmap.scaled(self.size, aspectMode=QtCore.Qt.AspectRatioMode.KeepAspectRatioByExpanding)
        self.qStreamingLabel.setPixmap(pixmap)

    def setStreamingImage(self, pixmap: QtGui.QPixmap):
        if self.device.deviceState == DeviceState.IDLE:
            return
        scaledPixmap = pixmap.scaled(self.size, aspectMode=QtCore.Qt.AspectRatioMode.KeepAspectRatioByExpanding)
        self.qStreamingLabel.setPixmap(scaledPixmap)
        self.setLayout(self.qvBox)
