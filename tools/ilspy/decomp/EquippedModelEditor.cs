using UnityEngine;

public class EquippedModelEditor : DebugMenu
{
	private EquippedModelProperties EquippedModelProperties;

	public EquippedModelEditor()
		: base(GameImpl.Translate("DEBUG_EquippedModelEditor"))
	{
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		EquippedModelProperties equippedModelProperties = null;
		Character character = Hud.Instance.LocalControlledCharacter;
		Equipment equipment = null;
		if (character != null)
		{
			equipment = character.EquippedItem;
			if (equipment != null)
			{
				equippedModelProperties = equipment.GetPrototype().EquippedModelProperties;
				if (equippedModelProperties == null && !string.IsNullOrEmpty(equipment.GetPrototype().EquippedModelName))
				{
					Story story = GameImpl.Instance.FindStoryThatOwnsEquipmentPrototype(equipment.GetPrototype());
					if (story != null)
					{
						if (story.PrefabSettings == null)
						{
							story.PrefabSettings = new PrefabSettings();
							story.PrefabSettings.FileName = story.Path + "/PrefabSettings.xml";
						}
						equippedModelProperties = new EquippedModelProperties();
						equippedModelProperties.Owner = story.PrefabSettings;
						equippedModelProperties.PrefabPath = equipment.GetPrototype().EquippedModelName;
						equipment.GetPrototype().EquippedModelProperties = equippedModelProperties;
						story.PrefabSettings.EquippedModels.Add(equippedModelProperties);
					}
				}
			}
		}
		if (equippedModelProperties == EquippedModelProperties)
		{
			return;
		}
		EquippedModelProperties = equippedModelProperties;
		Items.Clear();
		if (EquippedModelProperties == null)
		{
			return;
		}
		if (EquippedModelProperties.Owner != null)
		{
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Reload"), delegate
			{
				EquippedModelProperties.Owner.Reload();
			}));
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Save"), delegate
			{
				PrefabSettings.SaveToFile(EquippedModelProperties.Owner.FileName, EquippedModelProperties.Owner);
			}));
		}
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_Prefab"), EquippedModelProperties.PrefabPath));
		Items.Add(new DebugMenuItemEnum<EquippedAnim>(GameImpl.Translate("DEBUG_EquippedAnim"), EquippedAnim.Count, () => EquippedModelProperties.EquippedAnim, delegate(EquippedAnim v)
		{
			EquippedModelProperties.EquippedAnim = v;
		}));
		Items.Add(new DebugMenuItemEnum<Bone>(GameImpl.Translate("DEBUG_Bone"), Bone.Count, () => EquippedModelProperties.EquippedBone, delegate(Bone v)
		{
			EquippedModelProperties.EquippedBone = v;
			character.UnityReinit();
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Position"), () => EquippedModelProperties.LocalPos.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.LocalPos = StringUtil.ParseVector3(v, EquippedModelProperties.LocalPos);
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Rotation"), () => EquippedModelProperties.LocalRotation.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.LocalRotation = StringUtil.ParseVector3(v, EquippedModelProperties.LocalRotation);
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Scale"), () => EquippedModelProperties.LocalScale.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.LocalScale = StringUtil.ParseFloat(v, EquippedModelProperties.LocalScale);
		}));
		if (equipment is AmmoWeapon)
		{
			Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_MuzzlePos"), () => EquippedModelProperties.MuzzleLocalPos.ToString("F3"), delegate(string v)
			{
				EquippedModelProperties.MuzzleLocalPos = StringUtil.ParseVector3(v, EquippedModelProperties.MuzzleLocalPos);
			}));
		}
		if (equipment is Bow)
		{
			Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_ArrowPrefab"), () => EquippedModelProperties.LoadedAmmoPrefabPath, delegate(string v)
			{
				EquippedModelProperties.LoadedAmmoPrefabPath = v;
			}));
			Items.Add(new DebugMenuItemEnum<Bone>(GameImpl.Translate("DEBUG_ArrowBone"), Bone.Count, () => EquippedModelProperties.LoadedAmmoBone, delegate(Bone v)
			{
				EquippedModelProperties.LoadedAmmoBone = v;
			}));
			Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_ArrowPosition"), () => EquippedModelProperties.LoadedAmmoLocalPos.ToString("F3"), delegate(string v)
			{
				EquippedModelProperties.LoadedAmmoLocalPos = StringUtil.ParseVector3(v, EquippedModelProperties.LoadedAmmoLocalPos);
			}));
			Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_ArrowRotation"), () => EquippedModelProperties.LoadedAmmoLocalRotation.ToString("F3"), delegate(string v)
			{
				EquippedModelProperties.LoadedAmmoLocalRotation = StringUtil.ParseVector3(v, EquippedModelProperties.LoadedAmmoLocalRotation);
			}));
			Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_ArrowScale"), () => EquippedModelProperties.LoadedAmmoLocalScale.ToString("F3"), delegate(string v)
			{
				EquippedModelProperties.LoadedAmmoLocalScale = StringUtil.ParseFloat(v, EquippedModelProperties.LoadedAmmoLocalScale);
			}));
		}
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_RightHandIKPoint"), ""));
		Items.Add(new DebugMenuItemEnum<Bone>(GameImpl.Translate("DEBUG_Bone"), Bone.Count, () => EquippedModelProperties.RightHandIKPoint.Bone, delegate(Bone v)
		{
			EquippedModelProperties.RightHandIKPoint.Bone = v;
			character.UnityReinit();
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Position"), () => EquippedModelProperties.RightHandIKPoint.Pos.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.RightHandIKPoint.Pos = StringUtil.ParseVector3(v, EquippedModelProperties.RightHandIKPoint.Pos);
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_PosWeight"), 0f, 1f, () => EquippedModelProperties.RightHandIKPoint.PosWeight, delegate(float v)
		{
			EquippedModelProperties.RightHandIKPoint.PosWeight = v;
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Rotation"), () => EquippedModelProperties.RightHandIKPoint.Rot.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.RightHandIKPoint.Rot = StringUtil.ParseVector3(v, EquippedModelProperties.RightHandIKPoint.Rot);
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_RotWeight"), 0f, 1f, () => EquippedModelProperties.RightHandIKPoint.RotWeight, delegate(float v)
		{
			EquippedModelProperties.RightHandIKPoint.RotWeight = v;
		}));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_RightHandCrouchingIKPoint"), ""));
		Items.Add(new DebugMenuItemEnum<Bone>(GameImpl.Translate("DEBUG_Bone"), Bone.Count, () => EquippedModelProperties.CrouchingRightHandIKPoint.Bone, delegate(Bone v)
		{
			EquippedModelProperties.CrouchingRightHandIKPoint.Bone = v;
			character.UnityReinit();
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Position"), () => EquippedModelProperties.CrouchingRightHandIKPoint.Pos.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.CrouchingRightHandIKPoint.Pos = StringUtil.ParseVector3(v, EquippedModelProperties.CrouchingRightHandIKPoint.Pos);
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_PosWeight"), 0f, 1f, () => EquippedModelProperties.CrouchingRightHandIKPoint.PosWeight, delegate(float v)
		{
			EquippedModelProperties.CrouchingRightHandIKPoint.PosWeight = v;
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Rotation"), () => EquippedModelProperties.CrouchingRightHandIKPoint.Rot.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.CrouchingRightHandIKPoint.Rot = StringUtil.ParseVector3(v, EquippedModelProperties.CrouchingRightHandIKPoint.Rot);
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_RotWeight"), 0f, 1f, () => EquippedModelProperties.CrouchingRightHandIKPoint.RotWeight, delegate(float v)
		{
			EquippedModelProperties.CrouchingRightHandIKPoint.RotWeight = v;
		}));
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_LeftHandIKPoint"), ""));
		Items.Add(new DebugMenuItemEnum<Bone>(GameImpl.Translate("DEBUG_Bone"), Bone.Count, () => EquippedModelProperties.LeftHandIKPoint.Bone, delegate(Bone v)
		{
			EquippedModelProperties.LeftHandIKPoint.Bone = v;
			character.UnityReinit();
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Position"), () => EquippedModelProperties.LeftHandIKPoint.Pos.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.LeftHandIKPoint.Pos = StringUtil.ParseVector3(v, EquippedModelProperties.LeftHandIKPoint.Pos);
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_PosWeight"), 0f, 1f, () => EquippedModelProperties.LeftHandIKPoint.PosWeight, delegate(float v)
		{
			EquippedModelProperties.LeftHandIKPoint.PosWeight = v;
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Rotation"), () => EquippedModelProperties.LeftHandIKPoint.Rot.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.LeftHandIKPoint.Rot = StringUtil.ParseVector3(v, EquippedModelProperties.LeftHandIKPoint.Rot);
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_RotWeight"), 0f, 1f, () => EquippedModelProperties.LeftHandIKPoint.RotWeight, delegate(float v)
		{
			EquippedModelProperties.LeftHandIKPoint.RotWeight = v;
		}));
		Vector3 currentIKPointPosInBoneSpace = GetCurrentIKPointPosInBoneSpace(EquippedModelProperties.RunningRightHandIKPoint, Bone.RightHand, out var euler);
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_RightHandRunningIKPoint"), currentIKPointPosInBoneSpace.ToString() + " - " + euler.ToString()));
		Items.Add(new DebugMenuItemEnum<Bone>(GameImpl.Translate("DEBUG_Bone"), Bone.Count, () => EquippedModelProperties.RunningRightHandIKPoint.Bone, delegate(Bone v)
		{
			EquippedModelProperties.RunningRightHandIKPoint.Bone = v;
			character.UnityReinit();
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Position"), () => EquippedModelProperties.RunningRightHandIKPoint.Pos.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.RunningRightHandIKPoint.Pos = StringUtil.ParseVector3(v, EquippedModelProperties.RunningRightHandIKPoint.Pos);
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_PosWeight"), 0f, 1f, () => EquippedModelProperties.RunningRightHandIKPoint.PosWeight, delegate(float v)
		{
			EquippedModelProperties.RunningRightHandIKPoint.PosWeight = v;
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Rotation"), () => EquippedModelProperties.RunningRightHandIKPoint.Rot.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.RunningRightHandIKPoint.Rot = StringUtil.ParseVector3(v, EquippedModelProperties.RunningRightHandIKPoint.Rot);
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_RotWeight"), 0f, 1f, () => EquippedModelProperties.RunningRightHandIKPoint.RotWeight, delegate(float v)
		{
			EquippedModelProperties.RunningRightHandIKPoint.RotWeight = v;
		}));
		currentIKPointPosInBoneSpace = GetCurrentIKPointPosInBoneSpace(EquippedModelProperties.CrouchIdleRightHandIKPoint, Bone.RightHand, out euler);
		Items.Add(new DebugMenuItemText(GameImpl.Translate("DEBUG_RightHandCrouchIdleIKPoint"), currentIKPointPosInBoneSpace.ToString() + " - " + euler.ToString()));
		Items.Add(new DebugMenuItemEnum<Bone>(GameImpl.Translate("DEBUG_Bone"), Bone.Count, () => EquippedModelProperties.CrouchIdleRightHandIKPoint.Bone, delegate(Bone v)
		{
			EquippedModelProperties.CrouchIdleRightHandIKPoint.Bone = v;
			character.UnityReinit();
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Position"), () => EquippedModelProperties.CrouchIdleRightHandIKPoint.Pos.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.CrouchIdleRightHandIKPoint.Pos = StringUtil.ParseVector3(v, EquippedModelProperties.CrouchIdleRightHandIKPoint.Pos);
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_PosWeight"), 0f, 1f, () => EquippedModelProperties.CrouchIdleRightHandIKPoint.PosWeight, delegate(float v)
		{
			EquippedModelProperties.CrouchIdleRightHandIKPoint.PosWeight = v;
		}));
		Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_Rotation"), () => EquippedModelProperties.CrouchIdleRightHandIKPoint.Rot.ToString("F3"), delegate(string v)
		{
			EquippedModelProperties.CrouchIdleRightHandIKPoint.Rot = StringUtil.ParseVector3(v, EquippedModelProperties.CrouchIdleRightHandIKPoint.Rot);
		}));
		Items.Add(new DebugMenuFloatAdjuster(GameImpl.Translate("DEBUG_RotWeight"), 0f, 1f, () => EquippedModelProperties.CrouchIdleRightHandIKPoint.RotWeight, delegate(float v)
		{
			EquippedModelProperties.CrouchIdleRightHandIKPoint.RotWeight = v;
		}));
	}

	private Vector3 GetCurrentIKPointPosInBoneSpace(IKPoint iKPoint, Bone childBone, out Vector3 euler)
	{
		Character localControlledCharacter = Hud.Instance.LocalControlledCharacter;
		GameObject unityBone = localControlledCharacter.GetUnityBone(iKPoint.Bone);
		GameObject unityBone2 = localControlledCharacter.GetUnityBone(childBone);
		if (unityBone != null && unityBone2 != null)
		{
			Matrix4x4 mat = Matrix4x4.Inverse(unityBone.transform.localToWorldMatrix) * unityBone2.transform.localToWorldMatrix;
			euler = mat.rotation.eulerAngles;
			return mat.Translation();
		}
		euler = Vector3.zero;
		return Vector3.zero;
	}
}
