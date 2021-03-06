#pragma once
#include "Primitives/interface/BasicTypes.h"
#include "Graphics/GraphicsEngine/interface/GraphicsTypes.h"

namespace Diligent 
{
struct StateTransitionDescPassStruct
{
        IDeviceObject* pResourceBefore;
        IDeviceObject* pResource;
        Uint32 FirstMipLevel;
        Uint32 MipLevelsCount;
        Uint32 FirstArraySlice;
        Uint32 ArraySliceCount;
        RESOURCE_STATE OldState;
        RESOURCE_STATE NewState;
        STATE_TRANSITION_TYPE TransitionType;
        STATE_TRANSITION_FLAGS Flags;
};
}