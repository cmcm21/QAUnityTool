from PySide6 import QtWidgets, QtCore, QtGui
from Network.Clients.DeviceClient import DeviceClient, DeviceState
from UI.ProgressBar import ProgressBar
from Utils.StreamingVideoHelper import StreamingVideoHelper
from Utils.UIOrientation import Orientation

SIZE_ZERO = QtCore.QSize(0, 0)
LANDSCAPE_IMG_PATH = "Assets/NoStreamingImgLandscape.png"
PORTRAIT_IMG_PATH = "Assets/NoStreamingImgPortrait.png"


def _checkImageOrientation(pixmap: QtGui.QPixmap) -> Orientation:
    if pixmap.width() >= pixmap.height():
        return Orientation.LANDSCAPE
    else:
        return Orientation.PORTRAIT


class DeviceScreen(QtWidgets.QWidget):

    def __init__(self):
        super().__init__()
        self.device = None
        self.address = None
        self.landscapeSize = QtCore.QSize(400, 75)
        self.portraitSize = QtCore.QSize(200, 400)
        self.lastImgSize = SIZE_ZERO
        self.orientation = Orientation.LANDSCAPE

        self.qNameLabel = QtWidgets.QLabel(str(self.address))
        self.qStreamingLabel = QtWidgets.QLabel("Video Streaming")
        self.progressBar = ProgressBar(QtCore.QSize(435, 30))
        self.progressBar.setMaximumHeight(50)
        self.progressBar.setContentsMargins(40, 5, 0, 5)
        self.qvBox = QtWidgets.QVBoxLayout()
        self.qvBox.addWidget(self.qNameLabel, alignment=QtCore.Qt.AlignmentFlag.AlignHCenter)
        self.qvBox.addWidget(self.qStreamingLabel, alignment=QtCore.Qt.AlignmentFlag.AlignHCenter)
        self.qvBox.addWidget(self.progressBar)
        self.qvBox.setSpacing(1)
        self.qvBox.setContentsMargins(5, 5, 5, 5)
        self.progressBar.setVisible(False)

        self._showDefaultImage()
        self.setLayout(self.qvBox)

    def setDevice(self, device: DeviceClient):
        self.device = device
        self.address = device.address
        self.qNameLabel.setText(f"{device.hostname} {device.address}")
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
        self.hide()
        self._showDefaultImage()
        self.qNameLabel.setText("None")

    def _showDefaultImage(self):
        if self.orientation == Orientation.LANDSCAPE:
            imagePath = LANDSCAPE_IMG_PATH
            scaleFactor = self.landscapeSize
        else:
            imagePath = PORTRAIT_IMG_PATH
            scaleFactor = self.portraitSize

        pixmap = QtGui.QPixmap(imagePath)
        pixmap = pixmap.scaled(scaleFactor, aspectMode=QtCore.Qt.AspectRatioMode.KeepAspectRatio)
        self.qStreamingLabel.setPixmap(pixmap)

    def setStreamingImage(self, pixmap: QtGui.QPixmap):
        if self.device is None or self.device.deviceState == DeviceState.IDLE:
            return

        scaleFactor = self.landscapeSize \
            if (_checkImageOrientation(pixmap) == Orientation.LANDSCAPE) else self.portraitSize

        scaledPixmap = pixmap.scaled(scaleFactor, aspectMode=QtCore.Qt.AspectRatioMode.KeepAspectRatio)
        self.qStreamingLabel.setPixmap(scaledPixmap)
        self.setLayout(self.qvBox)

    def setOrientation(self, orientation: Orientation):
        self.orientation = orientation
        if self.device is None or self.device.deviceState == DeviceState.IDLE:
            self._showDefaultImage()
