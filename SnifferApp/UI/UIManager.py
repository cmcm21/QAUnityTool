from PySide6 import QtWidgets, QtCore
from PySide6.QtGui import QIcon, QAction
from UI.ServerWidget import ServerWidget
from UI.UILogger import UILogger
import platform


class UIManager:
    def __init__(self):
        self.guiApp = QtWidgets.QApplication([])
        self.guiApp.setStyle(platform.system())

        self.serverWidget = ServerWidget()
        self.uiLogger = UILogger()

        self.mainWindow = QtWidgets.QMainWindow()
        self.mainWindow.setWindowTitle("Sniffer Hub")

        self.vLayout = QtWidgets.QVBoxLayout()
        self.vLayout.addWidget(self.serverWidget, stretch=1)
        self.vLayout.addWidget(self.uiLogger)

        self.mainWindow.setCentralWidget(QtWidgets.QWidget())
        self.mainWindow.centralWidget().setLayout(self.vLayout)
        self.mainWindow.show()

    def execute(self):
        self.guiApp.exec()
