from PySide6 import QtWidgets, QtCore
from Network.Clients.DeviceClient import DeviceClient
from Network.Clients.StreamingClient import *
from PySide6.QtGui import QPixmap
from UI.DeviceScreen import DeviceScreen
from UI.UIVideoPlayer import VideoPlayer
from Utils.Settings import *
from UI import UIManager
from Utils.UIOrientation import Orientation


class DeviceWidget(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        self.deviceScreensRef: {str: DeviceScreen} = {}
        self.screens = []
        self.screensUsed = 0

        self._initButtons()
        self.streamingScreens = self._initStreamingScreens()
        self.streamingScrollArea = self._initScrollArea()
        self.videoPlayer = VideoPlayer()
        self.streamingLayout = self._initStreamingLayout()
        self.streamingWidget = QtWidgets.QWidget()
        self.streamingWidget.setLayout(self.streamingLayout)

        self.tab = self._initTab()

        self.container = QtWidgets.QVBoxLayout()
        self.buttonsLayout = self._setButtonsLayout()
        self.container.addWidget(self.tab)
        self.container.addLayout(self.buttonsLayout)
        self.setLayout(self.container)

    def setStreamingImage(self, pixmap: QPixmap, clientId: str):
        if clientId in self.deviceScreensRef:
            self.deviceScreensRef[clientId].setStreamingImage(pixmap)

    def _initButtons(self):
        self.recordBtn = QtWidgets.QPushButton("Record")
        self.stopBtn = QtWidgets.QPushButton("Stop")
        self.replayBtn = QtWidgets.QPushButton("Replay")
        self.stopReplayBtn = QtWidgets.QPushButton("Stop Replay")
        self.loadFileBtn = QtWidgets.QPushButton("Load")

        self.recordBtn.clicked.connect(self.onRecordBtnClicked)
        self.stopBtn.clicked.connect(self.onStopBtnClicked)
        self.loadFileBtn.clicked.connect(self.onLoadBtnClicked)
        self.replayBtn.clicked.connect(self.onReplayBtnClicked)

    def _initStreamingScreens(self) -> QtWidgets.QLayout:
        streamingContainer = QtWidgets.QGridLayout()
        row = 0
        column = 0
        for i in range(MAX_DEVICES):
            screenDevice = DeviceScreen()
            self.screens.append(screenDevice)
            streamingContainer.addWidget(screenDevice, row, column)
            column += 1
            if column > 1:
                row += 1
                column = 0

        streamingContainer.setSpacing(0)
        streamingContainer.setContentsMargins(0, 0, 0, 0)
        return streamingContainer

    def _initScrollArea(self) -> QtWidgets.QScrollArea:
        widget = QtWidgets.QWidget()
        scrollArea = QtWidgets.QScrollArea()
        scrollArea.setWidget(widget)
        widget.setLayout(self.streamingScreens)
        scrollArea.setVerticalScrollBarPolicy(QtCore.Qt.ScrollBarPolicy.ScrollBarAsNeeded)
        scrollArea.setHorizontalScrollBarPolicy(QtCore.Qt.ScrollBarPolicy.ScrollBarAsNeeded)
        scrollArea.setWidgetResizable(True)

        return scrollArea

    def _initStreamingLayout(self):
        vLayout = QtWidgets.QVBoxLayout()
        btnLayout = self._initStreamingButtons()

        vLayout.addLayout(btnLayout)
        vLayout.addWidget(self.streamingScrollArea)
        return vLayout

    def _initStreamingButtons(self) -> QtWidgets.QHBoxLayout:
        btnLayout = QtWidgets.QHBoxLayout()
        self.landscapeBtn = QtWidgets.QPushButton("Landscape")
        self.portraitBtn = QtWidgets.QPushButton("Portrait")

        btnLayout.addWidget(self.landscapeBtn, alignment=QtCore.Qt.AlignmentFlag.AlignVCenter)
        btnLayout.addWidget(self.portraitBtn, alignment=QtCore.Qt.AlignmentFlag.AlignVCenter)

        self.landscapeBtn.clicked.connect(self.onLandscapeBtnClicked)
        self.portraitBtn.clicked.connect(self.onPortraitBtnClicked)
        return btnLayout

    def _initTab(self) -> QtWidgets.QTabWidget:
        tab = QtWidgets.QTabWidget()
        tab.addTab(self.streamingWidget, "Streaming")
        tab.addTab(self.videoPlayer, "Video player")
        tab.setMovable(True)
        tab.setTabShape(QtWidgets.QTabWidget.TabShape.Triangular)

        return tab

    def _setButtonsLayout(self) -> QtWidgets.QLayout:
        buttonsLayout = QtWidgets.QHBoxLayout()
        buttonsLayout.addWidget(self.recordBtn, alignment=QtCore.Qt.AlignmentFlag.AlignVCenter)
        buttonsLayout.addWidget(self.stopBtn, alignment=QtCore.Qt.AlignmentFlag.AlignVCenter)
        buttonsLayout.addWidget(self.loadFileBtn,  alignment=QtCore.Qt.AlignmentFlag.AlignVCenter)
        buttonsLayout.addWidget(self.replayBtn,  alignment=QtCore.Qt.AlignmentFlag.AlignVCenter)
        buttonsLayout.addWidget(self.stopReplayBtn, alignment=QtCore.Qt.AlignmentFlag.AlignVCenter)

        buttonsLayout.setSpacing(5)
        buttonsLayout.setContentsMargins(50, 5, 50, 5)
        return buttonsLayout

    @QtCore.Slot()
    def onRecordBtnClicked(self):
        pass

    @QtCore.Slot()
    def onStopBtnClicked(self):
        return

    @QtCore.Slot()
    def onLoadBtnClicked(self):
        return

    @QtCore.Slot()
    def onReplayBtnClicked(self):
        return

    @QtCore.Slot()
    def onLandscapeBtnClicked(self):
        for screen in self.screens:
            screen.setOrientation(Orientation.LANDSCAPE)

    @QtCore.Slot()
    def onPortraitBtnClicked(self):
        for screen in self.screens:
            screen.setOrientation(Orientation.PORTRAIT)

    def onDeviceDisconnected(self, *args, **kwargs):
        if 'device' in kwargs:
            device = kwargs['device']
            if device.id in self.deviceScreensRef:
                self.deviceScreensRef[device.id].reset()
        if self.screensUsed > 0:
            self.screensUsed -= 1

    def noDevices(self):
        for key, value in self.deviceScreensRef.items():
            value.reset()

    def createDeviceScreen(self, device: DeviceClient):
        if self.screensUsed < MAX_DEVICES:
            screenDevice: DeviceScreen = self.screens[self.screensUsed]
            self.deviceScreensRef[device.id] = screenDevice
            screenDevice.setDevice(device)
            self.screensUsed += 1

    def updateDeviceScreen(self, *args, **kwargs):
        if 'device' in kwargs:
            device: DeviceClient = kwargs['device']
            if device.id in self.deviceScreensRef:
                self.deviceScreensRef[device.id].setDevice(device)

    def deviceSelected(self, devices: list[DeviceClient]):
        for device in devices:
            if device.id in self.deviceScreensRef:
                self.deviceScreensRef[device.id].show()

    def streamingClientConnected(self, client: StreamingClient, helper: StreamingVideoHelper):
        if client.id in self.deviceScreensRef:
            self.deviceScreensRef[client.id].setHelper(helper)

    def resetStreaming(self, address: str):
        if address in self.deviceScreensRef:
            self.deviceScreensRef[address].reset()

    def __del__(self):
        return
