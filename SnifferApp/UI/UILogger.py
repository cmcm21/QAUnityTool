from PySide6 import QtWidgets, QtCore


class UILogger(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        self.console = QtWidgets.QTextBrowser()
        self.clearBtn = QtWidgets.QPushButton("Clear Text")

        self.layout = QtWidgets.QVBoxLayout()
        self.layout.addWidget(self.console)
        self.layout.addWidget(self.clearBtn)

        self.clearBtn.clicked.connect(self.clear)
        self.setMaximumSize(QtCore.QSize(3000, 200))
        self.setLayout(self.layout)

    def appendText(self, text: str):
        self.console.append(text)
        return

    @QtCore.Slot()
    def clear(self):
        self.console.clear()
        return


