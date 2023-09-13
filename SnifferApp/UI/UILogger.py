from PySide6 import QtWidgets, QtCore


class UILogger(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        self.console = QtWidgets.QTextBrowser()
        self.clearBtn = QtWidgets.QPushButton("Clear Text")

        self.layout = QtWidgets.QVBoxLayout()
        self.layout.addWidget(self.console)
        self.layout.addWidget(self.clearBtn)

        self.console.textChanged.connect(self._setVScrollBarMaxValue)
        self.clearBtn.clicked.connect(self.clear)
        self.setMaximumSize(QtCore.QSize(3000, 200))
        self.setLayout(self.layout)

    def appendText(self, text: str):
        self.console.append(text)
        self._setVScrollBarMaxValue()

    def _onTextChanged(self):
        self._setVScrollBarMaxValue()

    def _setVScrollBarMaxValue(self):
        scrollBar = self.console.verticalScrollBar()
        if scrollBar:
            maxValue = scrollBar.maximum()
            self.console.verticalScrollBar().setValue(maxValue)


    @QtCore.Slot()
    def clear(self):
        self.console.clear()
        return


