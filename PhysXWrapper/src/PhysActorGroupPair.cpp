#include "StdAfx.h"
#include "..\include\PhysActorGroupPair.h"

namespace PhysXWrapper
{

PhysActorGroupPair::PhysActorGroupPair(NxActorGroupPair* pair)
:flags((ContactPairFlag)pair->flags), group0(pair->group0), group1(pair->group1)
{
}

}