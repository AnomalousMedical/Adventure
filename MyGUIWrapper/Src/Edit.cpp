#include "Stdafx.h"

extern "C" _AnomalousExport void Edit_setTextIntervalColor(MyGUI::Edit* edit, size_t start, size_t count, Color colour)
{
	edit->setTextIntervalColour(start, count, colour.toMyGUI());
}

extern "C" _AnomalousExport void Edit_setTextSelection(MyGUI::Edit* edit, size_t start, size_t end)
{
	edit->setTextSelection(start, end);
}

extern "C" _AnomalousExport void Edit_deleteTextSelection(MyGUI::Edit* edit)
{
	edit->deleteTextSelection();
}

extern "C" _AnomalousExport void Edit_setTextSelectionColor(MyGUI::Edit* edit, Color value)
{
	edit->setTextSelectionColour(value.toMyGUI());
}

extern "C" _AnomalousExport void Edit_insertText1(MyGUI::Edit* edit, String text)
{
	edit->insertText(text);
}

extern "C" _AnomalousExport void Edit_insertText2(MyGUI::Edit* edit, String text, size_t index)
{
	edit->insertText(text, index);
}

extern "C" _AnomalousExport void Edit_addText(MyGUI::Edit* edit, String text)
{
	edit->addText(text);
}

extern "C" _AnomalousExport void Edit_eraseText1(MyGUI::Edit* edit, size_t start)
{
	edit->eraseText(start);
}

extern "C" _AnomalousExport void Edit_eraseText2(MyGUI::Edit* edit, size_t start, size_t count)
{
	edit->eraseText(start, count);
}

extern "C" _AnomalousExport size_t Edit_getTextSelectionStart(MyGUI::Edit* edit)
{
	return edit->getTextSelectionStart();
}

extern "C" _AnomalousExport size_t Edit_getTextSelectionEnd(MyGUI::Edit* edit)
{
	return edit->getTextSelectionEnd();
}

extern "C" _AnomalousExport size_t Edit_getTextSelectionLength(MyGUI::Edit* edit)
{
	return edit->getTextSelectionLength();
}

extern "C" _AnomalousExport bool Edit_isTextSelection(MyGUI::Edit* edit)
{
	return edit->isTextSelection();
}

extern "C" _AnomalousExport void Edit_setTextCursor(MyGUI::Edit* edit, size_t index)
{
	edit->setTextCursor(index);
}

extern "C" _AnomalousExport size_t Edit_getTextCursor(MyGUI::Edit* edit)
{
	return edit->getTextCursor();
}

extern "C" _AnomalousExport size_t Edit_getTextLength(MyGUI::Edit* edit)
{
	return edit->getTextLength();
}

extern "C" _AnomalousExport void Edit_setOverflowToTheLeft(MyGUI::Edit* edit, bool value)
{
	edit->setOverflowToTheLeft(value);
}

extern "C" _AnomalousExport bool Edit_getOverflowToTheLeft(MyGUI::Edit* edit)
{
	return edit->getOverflowToTheLeft();
}

extern "C" _AnomalousExport void Edit_setMaxTextLength(MyGUI::Edit* edit, size_t value)
{
	edit->setMaxTextLength(value);
}

extern "C" _AnomalousExport size_t Edit_getMaxTextLength(MyGUI::Edit* edit)
{
	return edit->getMaxTextLength();
}

extern "C" _AnomalousExport void Edit_setEditReadOnly(MyGUI::Edit* edit, bool value)
{
	edit->setEditReadOnly(value);
}

extern "C" _AnomalousExport bool Edit_getEditReadOnly(MyGUI::Edit* edit)
{
	return edit->getEditReadOnly();
}

extern "C" _AnomalousExport void Edit_setEditPassword(MyGUI::Edit* edit, bool value)
{
	edit->setEditPassword(value);
}

extern "C" _AnomalousExport bool Edit_getEditPassword(MyGUI::Edit* edit)
{
	return edit->getEditPassword();
}

extern "C" _AnomalousExport void Edit_setEditMultiLine(MyGUI::Edit* edit, bool value)
{
	edit->setEditMultiLine(value);
}

extern "C" _AnomalousExport bool Edit_getEditMultiLine(MyGUI::Edit* edit)
{
	return edit->getEditMultiLine();
}

extern "C" _AnomalousExport void Edit_setEditStatic(MyGUI::Edit* edit, bool value)
{
	edit->setEditStatic(value);
}

extern "C" _AnomalousExport bool Edit_getEditStatic(MyGUI::Edit* edit)
{
	return edit->getEditStatic();
}

extern "C" _AnomalousExport void Edit_setPasswordChar(MyGUI::Edit* edit, char value)
{
	edit->setPasswordChar(value);
}

extern "C" _AnomalousExport char Edit_getPasswordChar(MyGUI::Edit* edit)
{
	return edit->getPasswordChar();
}

extern "C" _AnomalousExport void Edit_setEditWordWrap(MyGUI::Edit* edit, bool value)
{
	edit->setEditWordWrap(value);
}

extern "C" _AnomalousExport bool Edit_getEditWordWrap(MyGUI::Edit* edit)
{
	return edit->getEditWordWrap();
}

extern "C" _AnomalousExport void Edit_setTabPrinting(MyGUI::Edit* edit, bool value)
{
	edit->setTabPrinting(value);
}

extern "C" _AnomalousExport bool Edit_getTabPrinting(MyGUI::Edit* edit)
{
	return edit->getTabPrinting();
}

extern "C" _AnomalousExport bool Edit_getInvertSelected(MyGUI::Edit* edit)
{
	return edit->getInvertSelected();
}

extern "C" _AnomalousExport void Edit_setInvertSelected(MyGUI::Edit* edit, bool value)
{
	edit->setInvertSelected(value);
}

extern "C" _AnomalousExport void Edit_setVisibleVScroll(MyGUI::Edit* edit, bool value)
{
	edit->setVisibleVScroll(value);
}

extern "C" _AnomalousExport bool Edit_isVisibleVScroll(MyGUI::Edit* edit)
{
	return edit->isVisibleVScroll();
}

extern "C" _AnomalousExport size_t Edit_getVScrollRange(MyGUI::Edit* edit)
{
	return edit->getVScrollRange();
}

extern "C" _AnomalousExport size_t Edit_getVScrollPosition(MyGUI::Edit* edit)
{
	return edit->getVScrollPosition();
}

extern "C" _AnomalousExport void Edit_setVScrollPosition(MyGUI::Edit* edit, size_t index)
{
	edit->setVScrollPosition(index);
}

extern "C" _AnomalousExport void Edit_setVisibleHScroll(MyGUI::Edit* edit, bool value)
{
	edit->setVisibleHScroll(value);
}

extern "C" _AnomalousExport bool Edit_isVisibleHScroll(MyGUI::Edit* edit)
{
	return edit->isVisibleHScroll();
}

extern "C" _AnomalousExport size_t Edit_getHScrollRange(MyGUI::Edit* edit)
{
	return edit->getHScrollRange();
}

extern "C" _AnomalousExport size_t Edit_getHScrollPosition(MyGUI::Edit* edit)
{
	return edit->getHScrollPosition();
}

extern "C" _AnomalousExport void Edit_setHScrollPosition(MyGUI::Edit* edit, size_t index)
{
	edit->setHScrollPosition(index);
}