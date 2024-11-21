using System;

namespace CapRecAnimConv
{
	public enum OutType
	{
		JMA,
		JMM,
		JMT,
		JMO,
		JMR,
		JMRX,
		JMZ,
		JMW,
		TXT_Pos,
		TXT_PosRot
	}

	public enum TreeItemType
	{
		Directory,
		File
	}

	public enum PositionModType
	{
		Nodes,
		ObjectXYZ,
		ObjectXY,
		Custom
	}

	public enum RotationModType
	{
		Nodes,
		Object
	}

	[Flags]
	public enum ControlFlags
	{
		Crouch = 0x1,
		Jump = 0x2,
		UserAnimation1 = 0x4,
		UserAnimation2 = 0x8,
		IntegratedLight = 0x10,
		ExactFacing = 0x20,
		Action = 0x40,
		UseEquipment = 0x80,
		LookDontTurn = 0x100,
		ForceAlert = 0x200,
		Reload = 0x400,
		PrimaryTrigger = 0x800,
		SecondaryTrigger = 0x1000,
		ThrowGrenade = 0x2000,
		SwapWeapons = 0x4000,
	}
}
