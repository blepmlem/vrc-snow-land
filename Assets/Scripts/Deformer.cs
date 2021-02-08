using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class Deformer : UdonSharpBehaviour
{
	[SerializeField]
	private bool _isPlayerTracker;

	[SerializeField]
	private ParticleSystem _particles;

	[SerializeField]
	private float _hitRadius = .7f;

	private ParticleSystem.EmissionModule _emission;
	private int _layer;

	private VRCPlayerApi _player;

	private Transform _target;

	private float _timeSliceInterval = .1f;
	private float _timeSliceTimer;
	private void Start()
	{
		_emission = _particles.emission;
		_emission.enabled = false;
		_layer = 1 << 23; //"Deformable"
		if (!_isPlayerTracker)
		{
			var t = transform;
			_target = t.parent;
			t.parent = null;
			t.rotation = Quaternion.identity;
		}
	}

	private void Update()
	{
		_timeSliceTimer += Time.deltaTime;
		if (_timeSliceTimer < _timeSliceInterval)
		{
			return;
		}

		_timeSliceTimer = 0;
		if (!_isPlayerTracker)
		{
			transform.position = _target.position;
		}
		else if (_player != null)
		{
			transform.position = _player.GetPosition();
		}
		else
		{
			enabled = false;
			return;
		}

		bool hit = Physics.CheckSphere(transform.position, _hitRadius, _layer);
		_emission.enabled = hit;
	}

	public void SetPlayer(VRCPlayerApi player)
	{
		_player = player;
		enabled = true;
		Debug.Log($"{gameObject.name} has been assigned to {player.displayName}");
	}

	public void RemovePlayer()
	{
		_player = null;
		enabled = false;
	}
}