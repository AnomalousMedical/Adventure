#include "Stdafx.h"

extern "C" _AnomalousExport MyGUI::Gui* Gui_Create()
{
	return new MyGUI::Gui();
}

extern "C" _AnomalousExport void Gui_Delete(MyGUI::Gui* gui)
{
	delete gui;
}

extern "C" _AnomalousExport void Gui_initialize(MyGUI::Gui* gui, String coreConfig, String logFile)
{
	gui->initialise(coreConfig, logFile);
}

extern "C" _AnomalousExport void Gui_shutdown(MyGUI::Gui* gui)
{
	gui->shutdown();
}

extern "C" _AnomalousExport MyGUI::Widget* Gui_createWidgetT(MyGUI::Gui* gui, String type, String skin, int left, int top, int width, int height, MyGUI::Align align, String layer, String name)
{
	return gui->createWidgetT(type, skin, left, top, width, height, align, layer, name);
}

extern "C" _AnomalousExport bool Gui_injectMouseMove(MyGUI::Gui* gui, int absx, int absy, int absz)
{
	return gui->injectMouseMove(absx, absy, absz);
}

extern "C" _AnomalousExport bool Gui_injectMousePress(MyGUI::Gui* gui, int absx, int absy, MyGUI::MouseButton id)
{
	return gui->injectMousePress(absx, absy, id);
}

extern "C" _AnomalousExport bool Gui_injectMouseRelease(MyGUI::Gui* gui, int absx, int absy, MyGUI::MouseButton id)
{
	return gui->injectMouseRelease(absx, absy, id);
}

extern "C" _AnomalousExport bool Gui_injectKeyPress(MyGUI::Gui* gui, MyGUI::KeyCode key, uint text)
{
	return gui->injectKeyPress(key, text);
}

extern "C" _AnomalousExport bool Gui_injectKeyRelease(MyGUI::Gui* gui, MyGUI::KeyCode key)
{
	return gui->injectKeyRelease(key);
}

extern "C" _AnomalousExport void Gui_setVisiblePointer(MyGUI::Gui* gui, bool visible)
{
	gui->setVisiblePointer(visible);
}

extern "C" _AnomalousExport bool Gui_isVisiblePointer(MyGUI::Gui* gui)
{
	return gui->isVisiblePointer();
}