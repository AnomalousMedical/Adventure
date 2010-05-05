#pragma once
#include "TypedConstraintElement.h"

using namespace System;
using namespace Engine;
using namespace Engine::ObjectManagement;

namespace BulletPlugin
{

ref class Generic6DofConstraintDefinition;
ref class RigidBody;
ref class BulletScene;

[Engine::Attributes::NativeSubsystemType]
public ref class Generic6DofConstraintElement : public TypedConstraintElement
{
private:
	btGeneric6DofConstraint* dof;

public:
	Generic6DofConstraintElement(Generic6DofConstraintDefinition^ definition, SimObjectBase^ instance, RigidBody^ rbA, RigidBody^ rbB, BulletScene^ scene);

	virtual ~Generic6DofConstraintElement(void);

	virtual SimElementDefinition^ saveToDefinition() override;

	void setFrameOffsetA(Vector3 origin);

	void setFrameOffsetA(Quaternion basis);

	void setFrameOffsetA(Vector3 origin, Quaternion basis);

	void setFrameOffsetB(Vector3 origin);

	void setFrameOffsetB(Quaternion basis);

	void setFrameOffsetB(Vector3 origin, Quaternion basis);

	Vector3 getFrameOffsetOriginA();

	Quaternion getFrameOffsetBasisA();

	Vector3 getFrameOffsetOriginB();

	Quaternion getFrameOffsetBasisB();

	void setLimit(int axis, float lo, float hi);

	void setLinearLowerLimit(Vector3 linearLower);

	void setLinearUpperLimit(Vector3 linearUpper);

	void setAngularLowerLimit(Vector3 angularLower);

	void setAngularUpperLimit(Vector3 angularUpper);

	enum class Param
	{
		CONSTRAINT_STOP_ERP = BT_CONSTRAINT_STOP_ERP,
		CONSTRAINT_STOP_CFM = BT_CONSTRAINT_STOP_CFM,
		CONSTRAINT_CFM = BT_CONSTRAINT_CFM,
	};

	void setParam(Param num, float value, int axis);
};

}