#pragma once

#include "AutoPtr.h"
#include "PhysJointDesc.h"
#include "NxPointInPlaneJointDesc.h"

namespace Physics
{

/// <summary>
/// Wrapper for NxPointInPlaneJointDesc.
/// Describes PointInPlaneJoint.
/// </summary>
public ref class PhysPointInPlaneJointDesc : public PhysJointDesc
{
internal:
	AutoPtr<NxPointInPlaneJointDesc> joint;

public:
	/// <summary>
	/// Constructor
	/// </summary>
	PhysPointInPlaneJointDesc();
};

}