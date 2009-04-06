#include "StdAfx.h"
#include "..\include\PhysSphereShapeDesc.h"
#include "NxPhysics.h"

namespace PhysXWrapper
{

PhysSphereShapeDesc::PhysSphereShapeDesc()
:sphereShape(new NxSphereShapeDesc()),
PhysShapeDesc(sphereShape.Get())
{
}

float PhysSphereShapeDesc::Radius::get() 
{
	return sphereShape->radius;
}

void PhysSphereShapeDesc::Radius::set(float value) 
{
	sphereShape->radius = value;
}

}