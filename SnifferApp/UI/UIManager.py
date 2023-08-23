from PySide6 import QtWidgets, QtCore, QtGui
from UI.ServerWidget import ServerWidget
from UI.UILogger import UILogger
from UI.DeviceWidget import DeviceWidget
from UI.SplashScreen import SplashScreen
from Network.DeviceClient import DeviceClient
import platform


class UIManager:
    def __init__(self):
        self.guiApp = QtWidgets.QApplication([])
        self.guiApp.setStyle(platform.system())

        self.serverWidget = ServerWidget()
        self.uiLogger = UILogger()
        self.deviceWidget = DeviceWidget()
        self.splashScreen = SplashScreen()

        self._initMainWindow()

        self.deviceWidget.hide()
        self._connectEvents()

    def _initMainWindow(self):
        self.mainWindow = QtWidgets.QMainWindow()
        self.mainWindow.setWindowTitle("Sniffer Hub")

        self.hLayout = QtWidgets.QHBoxLayout()
        self.hLayout.addStretch()
        self.hLayout.addWidget(self.serverWidget)
        self.hLayout.addWidget(self.deviceWidget)

        self.vLayout = QtWidgets.QVBoxLayout()
        self.vLayout.addLayout(self.hLayout)
        self.vLayout.addWidget(self.uiLogger)

        self.mainWindow.setCentralWidget(QtWidgets.QWidget())
        self.mainWindow.centralWidget().setLayout(self.vLayout)
        self.hLayout.addStretch()
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

    def onFileSizeReceived(self, *args, **kwargs):
        size = kwargs["size"]
        self.splashScreen.setSplashScreen(f"Saving File of size{size}")
        self.splashScreen.show()

    async def onFileReceivedFinished(self, *args, **kwargs):
        await self.splashScreen.customHide(self.mainWindow)

    def GetWidget(self) -> QtWidgets.QWidget:
        return self.mainWindow.centralWidget()

    def execute(self):
        self.guiApp.exec()
