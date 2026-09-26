using System;
using UnityEngine;

[Serializable]
public class RagdollSettings : ScriptableObject
{
	[Serializable]
	public class RigidBodySettings
	{
		public float angularDrag;

		public float drag;

		public Vector3 centerOfMass;

		public float mass;

		public CollisionDetectionMode collisionDetectionMode;

		public bool useGravity;

		public float sleepThreshold;
	}

	[Serializable]
	public class BoxColliderSettings
	{
		public Vector3 center;

		public Vector3 size;

		public bool isTrigger;

		public PhysicsMaterial material;

		public float contactOffset;

		public bool enabled;
	}

	[Serializable]
	public class CapsuleColliderSettings
	{
		public Vector3 center;

		public int direction;

		public float height;

		public float radius;

		public bool isTrigger;

		public PhysicsMaterial material;

		public float contactOffset;

		public bool enabled;
	}

	[Serializable]
	public class SphereColliderSettings
	{
		public Vector3 center;

		public float radius;

		public bool isTrigger;

		public PhysicsMaterial material;

		public float contactOffset;

		public bool enabled;
	}

	[Serializable]
	public class JointSettings
	{
		public Vector3 anchor;

		public bool autoConfigureConnectedAnchor;

		public Vector3 axis;

		public float breakForce;

		public float breakTorque;

		public Vector3 connectedAnchor;

		public bool enableCollision;

		public bool enablePreprocessing;

		public bool enableProjection;

		public SoftJointLimitSettings highTwistLimit;

		public SoftJointLimitSettings lowTwistLimit;

		public float projectionAngle;

		public float projectionDistance;

		public SoftJointLimitSettings swing1Limit;

		public SoftJointLimitSettings swing2Limit;

		public Vector3 swingAxis;

		public SoftJointLimitSpringSettings swingLimitSpring;

		public SoftJointLimitSpringSettings twistLimitSpring;

		public bool enabled;
	}

	[Serializable]
	public struct SoftJointLimitSettings
	{
		public float limit;

		public float bounciness;

		public float contactDistance;

		public void CopyFrom(SoftJointLimit from)
		{
			limit = from.limit;
			bounciness = from.bounciness;
			contactDistance = from.contactDistance;
		}

		public SoftJointLimit CopyToSoftJointLimit()
		{
			return new SoftJointLimit
			{
				limit = limit,
				bounciness = bounciness,
				contactDistance = contactDistance
			};
		}
	}

	[Serializable]
	public struct SoftJointLimitSpringSettings
	{
		public float spring;

		public float damper;

		public void CopyFrom(SoftJointLimitSpring from)
		{
			spring = from.spring;
			damper = from.damper;
		}

		public SoftJointLimitSpring CopyToSoftJointLimitSpring()
		{
			return new SoftJointLimitSpring
			{
				spring = spring,
				damper = damper
			};
		}
	}

	[Serializable]
	public class BoneSettings
	{
		public string Name;

		public string ConnectedName;

		public RigidBodySettings RigidBody;

		public BoxColliderSettings BoxCollider;

		public CapsuleColliderSettings CapsuleCollider;

		public SphereColliderSettings SphereCollider;

		public JointSettings Joint;

		public void CopyFrom(GameObject fromObj, GameObject rootObj)
		{
			Rigidbody component = fromObj.GetComponent<Rigidbody>();
			BoxCollider component2 = fromObj.GetComponent<BoxCollider>();
			CapsuleCollider component3 = fromObj.GetComponent<CapsuleCollider>();
			SphereCollider component4 = fromObj.GetComponent<SphereCollider>();
			CharacterJoint component5 = fromObj.GetComponent<CharacterJoint>();
			RigidBody = new RigidBodySettings();
			RigidBody.angularDrag = component.angularDamping;
			RigidBody.drag = component.linearDamping;
			RigidBody.centerOfMass = component.centerOfMass;
			RigidBody.mass = component.mass;
			RigidBody.collisionDetectionMode = component.collisionDetectionMode;
			RigidBody.useGravity = component.useGravity;
			RigidBody.sleepThreshold = component.sleepThreshold;
			if (component2 != null && component2.enabled)
			{
				BoxCollider = new BoxColliderSettings();
				BoxCollider.center = component2.center;
				BoxCollider.size = component2.size;
				BoxCollider.isTrigger = component2.isTrigger;
				BoxCollider.material = component2.material;
				BoxCollider.contactOffset = component2.contactOffset;
				BoxCollider.enabled = true;
			}
			if (component3 != null && component3.enabled)
			{
				CapsuleCollider = new CapsuleColliderSettings();
				CapsuleCollider.center = component3.center;
				CapsuleCollider.direction = component3.direction;
				CapsuleCollider.height = component3.height;
				CapsuleCollider.radius = component3.radius;
				CapsuleCollider.isTrigger = component3.isTrigger;
				CapsuleCollider.material = component3.material;
				CapsuleCollider.contactOffset = component3.contactOffset;
				CapsuleCollider.enabled = true;
			}
			if (component4 != null && component4.enabled)
			{
				SphereCollider = new SphereColliderSettings();
				SphereCollider.center = component4.center;
				SphereCollider.radius = component4.radius;
				SphereCollider.isTrigger = component4.isTrigger;
				SphereCollider.material = component4.material;
				SphereCollider.contactOffset = component4.contactOffset;
				SphereCollider.enabled = true;
			}
			if (component5 != null)
			{
				Joint = new JointSettings();
				Joint.anchor = component5.anchor;
				Joint.autoConfigureConnectedAnchor = component5.autoConfigureConnectedAnchor;
				Joint.axis = component5.axis;
				Joint.breakForce = component5.breakForce;
				Joint.breakTorque = component5.breakTorque;
				Joint.connectedAnchor = component5.connectedAnchor;
				Joint.enableCollision = component5.enableCollision;
				Joint.enablePreprocessing = component5.enablePreprocessing;
				Joint.enableProjection = component5.enableProjection;
				Joint.highTwistLimit.CopyFrom(component5.highTwistLimit);
				Joint.lowTwistLimit.CopyFrom(component5.lowTwistLimit);
				Joint.projectionAngle = component5.projectionAngle;
				Joint.projectionDistance = component5.projectionDistance;
				Joint.swing1Limit.CopyFrom(component5.swing1Limit);
				Joint.swing2Limit.CopyFrom(component5.swing2Limit);
				Joint.swingAxis = component5.swingAxis;
				Joint.swingLimitSpring.CopyFrom(component5.swingLimitSpring);
				Joint.twistLimitSpring.CopyFrom(component5.twistLimitSpring);
				Joint.enabled = true;
				ConnectedName = GetGameObjectPath(component5.connectedBody.gameObject, rootObj);
			}
		}

		private string GetGameObjectPath(GameObject obj, GameObject rootObj)
		{
			string text = obj.name;
			while (obj != rootObj && obj.transform.parent != null)
			{
				obj = obj.transform.parent.gameObject;
				text = obj.name + "/" + text;
			}
			return text;
		}

		public void CopyTo(GameObject toObj, GameObject connectedObj, PhysicsMaterial materialOverride)
		{
			Rigidbody rigidbody = toObj.GetComponent<Rigidbody>();
			if (rigidbody == null)
			{
				rigidbody = toObj.AddComponent<Rigidbody>();
			}
			rigidbody.angularDamping = RigidBody.angularDrag;
			rigidbody.linearDamping = RigidBody.drag;
			rigidbody.centerOfMass = RigidBody.centerOfMass;
			rigidbody.mass = RigidBody.mass;
			rigidbody.collisionDetectionMode = RigidBody.collisionDetectionMode;
			rigidbody.isKinematic = true;
			rigidbody.detectCollisions = true;
			rigidbody.useGravity = RigidBody.useGravity;
			rigidbody.sleepThreshold = RigidBody.sleepThreshold;
			if (BoxCollider != null && BoxCollider.enabled)
			{
				BoxCollider boxCollider = toObj.GetComponent<BoxCollider>();
				if (boxCollider == null)
				{
					boxCollider = toObj.AddComponent<BoxCollider>();
				}
				boxCollider.center = BoxCollider.center;
				boxCollider.size = BoxCollider.size;
				boxCollider.isTrigger = BoxCollider.isTrigger;
				boxCollider.material = ((materialOverride != null) ? materialOverride : BoxCollider.material);
				boxCollider.contactOffset = BoxCollider.contactOffset;
				boxCollider.enabled = true;
			}
			if (CapsuleCollider != null && CapsuleCollider.enabled)
			{
				CapsuleCollider capsuleCollider = toObj.GetComponent<CapsuleCollider>();
				if (capsuleCollider == null)
				{
					capsuleCollider = toObj.AddComponent<CapsuleCollider>();
				}
				capsuleCollider.center = CapsuleCollider.center;
				capsuleCollider.direction = CapsuleCollider.direction;
				capsuleCollider.height = CapsuleCollider.height;
				capsuleCollider.radius = CapsuleCollider.radius;
				capsuleCollider.isTrigger = CapsuleCollider.isTrigger;
				capsuleCollider.material = ((materialOverride != null) ? materialOverride : CapsuleCollider.material);
				capsuleCollider.contactOffset = CapsuleCollider.contactOffset;
				capsuleCollider.enabled = true;
			}
			if (SphereCollider != null && SphereCollider.enabled)
			{
				SphereCollider sphereCollider = toObj.GetComponent<SphereCollider>();
				if (sphereCollider == null)
				{
					sphereCollider = toObj.AddComponent<SphereCollider>();
				}
				sphereCollider.center = SphereCollider.center;
				sphereCollider.radius = SphereCollider.radius;
				sphereCollider.isTrigger = SphereCollider.isTrigger;
				sphereCollider.material = ((materialOverride != null) ? materialOverride : SphereCollider.material);
				sphereCollider.contactOffset = SphereCollider.contactOffset;
				sphereCollider.enabled = true;
			}
			if (Joint != null && Joint.enabled)
			{
				CharacterJoint characterJoint = toObj.GetComponent<CharacterJoint>();
				if (characterJoint == null)
				{
					characterJoint = toObj.AddComponent<CharacterJoint>();
				}
				characterJoint.anchor = Joint.anchor;
				characterJoint.autoConfigureConnectedAnchor = Joint.autoConfigureConnectedAnchor;
				characterJoint.axis = Joint.axis;
				characterJoint.breakForce = Joint.breakForce;
				characterJoint.breakTorque = Joint.breakTorque;
				characterJoint.connectedAnchor = Joint.connectedAnchor;
				characterJoint.connectedBody = connectedObj.GetComponent<Rigidbody>();
				characterJoint.enableCollision = Joint.enableCollision;
				characterJoint.enablePreprocessing = Joint.enablePreprocessing;
				characterJoint.enableProjection = Joint.enableProjection;
				characterJoint.highTwistLimit = Joint.highTwistLimit.CopyToSoftJointLimit();
				characterJoint.lowTwistLimit = Joint.lowTwistLimit.CopyToSoftJointLimit();
				characterJoint.projectionAngle = Joint.projectionAngle;
				characterJoint.projectionDistance = Joint.projectionDistance;
				characterJoint.swing1Limit = Joint.swing1Limit.CopyToSoftJointLimit();
				characterJoint.swing2Limit = Joint.swing2Limit.CopyToSoftJointLimit();
				characterJoint.swingAxis = Joint.swingAxis;
				characterJoint.swingLimitSpring = Joint.swingLimitSpring.CopyToSoftJointLimitSpring();
				characterJoint.twistLimitSpring = Joint.twistLimitSpring.CopyToSoftJointLimitSpring();
			}
		}
	}

	public BoneSettings[] Bones = new BoneSettings[0];

	public BoneSettings FindBone(string name)
	{
		BoneSettings[] bones = Bones;
		foreach (BoneSettings boneSettings in bones)
		{
			if (boneSettings.Name == name)
			{
				return boneSettings;
			}
		}
		return null;
	}
}
