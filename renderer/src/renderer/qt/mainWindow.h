#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include "gpuContext.h"

#include <QWindow>

namespace melown
{
    class MapFoundation;
    class Fetcher;
}

class MainWindow : public QWindow
{
public:
    MainWindow();
    ~MainWindow();

    bool event(QEvent *event);
    void initialize();
    void tick();

    Gl *gl;
    melown::MapFoundation *map;

    melown::Fetcher *fetcher;

private:
    void mouseMove(class QMouseEvent *event);
    void mousePress(class QMouseEvent *event);
    void mouseRelease(class QMouseEvent *event);

    bool isMouseDetached;
    QPoint mouseLastPosition;
    QPoint mouseOriginalPosition;
};

#endif
