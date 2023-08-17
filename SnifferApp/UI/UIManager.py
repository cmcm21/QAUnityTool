from PySide6 import QtWidgets, QtCore
from PySide6.QtGui import QIcon, QAction
from UI.ServerWidget import ServerWidget
from UI.UILogger import UILogger
from UI.DeviceWidget import DeviceWidget
import platform
from Network.DeviceManager import DeviceManager


class UIManager:
    def __init__(self):
        self.guiApp = QtWidgets.QApplication([])
        self.guiApp.setStyle(platform.system())

        self.serverWidget = ServerWidget()
        self.uiLogger = UILogger()
        self.deviceWidget = DeviceWidget()

        self._initMainWindow()

        self.deviceWidget.hide()
        self._connectEvents()

    def _initMainWindow(self):
        self.mainWindow = QtWidgets.QMainWindow()
        self.mainWindow.setWindowTitle("Sniffer Hub")

        self.hLayout = QtWidgets.QHBoxLayout()
        self.hLayout.addWidget(self.serverWidget)
        self.hLayout.addWidget(self.deviceWidget)

        self.vLayout = QtWidgets.QVBoxLayout()
        self.vLayout.addLayout(self.hLayout)
        self.vLayout.addWidget(self.uiLogger)

        self.mainWindow.setCentralWidget(QtWidgets.QWidget())
        self.mainWindow.centralWidget().setLayout(self.vLayout)
        self.mainWindow.show()

    def _connectEvents(self):
            self.serverWidget.deviceSelectedChanged += self._onDeviceSelectedChanged

    def _onDeviceSelectedChanged(self, *args, **kwargs):
        device: DeviceManager = kwargs["device"]
        self.uiLogger.appendText("Selected device {}".format(device.address))
        self.deviceWidget.deviceSelected(device)
        return

    def execute(self):
        self.guiApp.exec()
