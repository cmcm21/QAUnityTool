from PySide6 import QtWidgets, QtCore, QtGui
from UI.ServerWidget import ServerWidget
from UI.UILogger import UILogger
from UI.DeviceWidget import DeviceWidget
from UI.ProgressBar import ProgressBar
from Utils.Events import Event
from Network.Clients.DeviceClient import DeviceClient
import platform


def setWidgetSize(size: QtCore.QSize, *args):
    for widget in args:
        widget.setMinimumSize(size)
        widget.setMaximumSize(size)


class MainWindow(QtWidgets.QMainWindow):
    def __init__(self):
        super().__init__()
        self.onCloseEvent = Event()

    def closeEvent(self, event: QtGui.QCloseEvent) -> None:
        self.onCloseEvent()
        super().closeEvent(event)


class UIManager:
    def __init__(self):
        self.guiApp = QtWidgets.QApplication([])
        self.guiApp.setStyle(platform.system())
        self.guiApp.quitOnLastWindowClosed()

        self.serverWidget = ServerWidget()
        self.uiLogger = UILogger()
        self.deviceWidget = DeviceWidget()
        self.fileTransferringProgressbar = ProgressBar()
        self.onApplicationQuitEvent = Event()

        self._initMainWindow()

        self.fileTransferringProgressbar.setVisible(False)
        self._connectEvents()
        self.mainWindow.onCloseEvent += self._onWindowsQuit

    def _onWindowsQuit(self, *args, **kwargs):
        self.uiLogger.appendText("Closing Application... please wait")
        self.onApplicationQuitEvent(args, kwargs)

    def _initMainWindow(self):
        self.mainWindow = MainWindow()
        self.mainWindow.setWindowTitle("Sniffer Hub")

        self.hLayout = QtWidgets.QHBoxLayout()
        self.hLayout.addWidget(self.serverWidget)
        self.hLayout.addWidget(self.deviceWidget)

        self.vLayout = QtWidgets.QVBoxLayout()
        self.vLayout.addLayout(self.hLayout)
        self.vLayout.addWidget(self.uiLogger)
        self.vLayout.addWidget(self.fileTransferringProgressbar, alignment=QtCore.Qt.AlignmentFlag.AlignJustify)

        self.mainWindow.setCentralWidget(QtWidgets.QWidget())
        self.mainWindow.centralWidget().setLayout(self.vLayout)
        self.mainWindow.setMinimumWidth(1300)
        self.mainWindow.showNormal()

    def _connectEvents(self):
        self.serverWidget.devicesSelectedChanged += self._onDeviceSelectedChanged
        self.serverWidget.clearSelectionEvent += self._onSelectionClear

    def _onDeviceSelectedChanged(self, *args, **kwargs):
        devices: list[DeviceClient] = kwargs["devices"]
        if len(devices) <= 0:
            return
        allIds = ""
        for device in devices:
            if allIds != "":
                allIds += ", "
            allIds += f"{device.id}"
        self.uiLogger.appendText("Selected devices {}".format(allIds))
        self.deviceWidget.deviceSelected(devices)

    def _onSelectionClear(self, *args, **kwargs):
        return

    def GetWidget(self) -> QtWidgets.QWidget:
        return self.mainWindow.centralWidget()

    def execute(self):
        self.guiApp.exec()
