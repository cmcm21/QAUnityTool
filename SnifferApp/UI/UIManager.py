from PySide6 import QtWidgets, QtCore
from UI.ServerWidget import ServerWidget
from UI.UILogger import UILogger
from UI.DeviceWidget import DeviceWidget
from UI.ProgressBar import ProgressBar
from Network.Clients.DeviceClient import DeviceClient
import platform


class UIManager:
    def __init__(self):
        self.guiApp = QtWidgets.QApplication([])
        self.guiApp.setStyle(platform.system())

        self.serverWidget = ServerWidget()
        self.uiLogger = UILogger()
        self.deviceWidget = DeviceWidget()
        self.renderingProgressBar = ProgressBar()
        self.fileTransferringProgressbar = ProgressBar()

        self._initMainWindow()

        #self.deviceWidget.hide()
        self.renderingProgressBar.hide()
        self.fileTransferringProgressbar.hide()
        self._connectEvents()

    def _initMainWindow(self):
        self.mainWindow = QtWidgets.QMainWindow()
        self.mainWindow.setWindowTitle("Sniffer Hub")

        self.hLayout = QtWidgets.QHBoxLayout()
        self.hLayout.addWidget(self.serverWidget)
        self.hLayout.addWidget(self.deviceWidget)
        self.hLayout.addStretch()

        self.vLayout = QtWidgets.QVBoxLayout()
        self.vLayout.addLayout(self.hLayout)
        self.vLayout.addWidget(self.uiLogger)
        self.vLayout.addWidget(self.renderingProgressBar)
        self.vLayout.addWidget(self.fileTransferringProgressbar)

        self.mainWindow.setCentralWidget(QtWidgets.QWidget())
        self.mainWindow.centralWidget().setLayout(self.vLayout)
        self.mainWindow.resize(QtCore.QSize(1100, 600))
        self.mainWindow.show()

    def _connectEvents(self):
        self.serverWidget.deviceSelectedChanged += self._onDeviceSelectedChanged
        self.serverWidget.clearSelectionEvent += self._onSelectionClear

    def _onDeviceSelectedChanged(self, *args, **kwargs):
        device: DeviceClient = kwargs["device"]
        self.uiLogger.appendText("Selected device {}".format(device.address))
        self.deviceWidget.deviceSelected(device)
        return

    def _onSelectionClear(self, *args, **kwargs):
        return

    def GetWidget(self) -> QtWidgets.QWidget:
        return self.mainWindow.centralWidget()

    def execute(self):
        self.guiApp.exec()
