#pragma once

using namespace Engine;
using namespace System;

namespace BulletPlugin
{

ref class BulletShapeRepository;
ref class BulletShapeBuilder;

ref class BulletShapeFileManager : ShapeFileManager
{
private:
	BulletShapeRepository^ shapeRepository;
	BulletShapeBuilder^ shapeBuilder;

protected:
	/// <summary>
    /// Called before shapes are loaded. If a cooker or anything else needs
    /// to be initialized do it here.
    /// </summary>
    virtual void loadStarted() override;

    /// <summary>
    /// Called when shapes are finished being loaded. If any cleanup needs
    /// to be done do it here.
    /// </summary>
    virtual void loadEnded() override;

public:
	BulletShapeFileManager(void);
};

}