using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Lean.Common;
using MoreMountains.NiceVibrations;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

public class GroupControll : MonoBehaviour
{
    [SerializeField] private Formation _radialFormation;
    [SerializeField] private GameObject _unitPrefab;
    [SerializeField] private float _unitSpeed = 2;
    [SerializeField] private int MoveForce;
  //  [SerializeField] private FloatVariableHolder CurrentWeight;
    private FormationBase _formation;
    private FloatingJoystick _joystick;
    private bool _returnPos;
    private List<Rigidbody> _spawnedUnitsRigidbodies = new List<Rigidbody>();
    private List<CharacterAnimatorController> _animatorControllers = new List<CharacterAnimatorController>();
    private List<bool> _UnitInsideOfGroup = new List<bool>();
    private Rigidbody GroupRigidbody;
    private Quaternion quaternion;
    private int CurrentFirst;
    private int CurrentMovePoint;
    private bool GroupFormationActive;
    private LevelManager _levelManager;
    private float firstSpeed;
     


    private readonly List<GameObject> _spawnedUnits = new List<GameObject>();
    private List<Vector3> _points = new List<Vector3>();
    private Transform _parent;
    [SerializeField] private GameActiveState GameActive;


    private void Awake()
    {
        firstSpeed = _unitSpeed; 
        _parent = new GameObject("Unit Parent").transform;
        GameActive.SetValue(false);
        _points = _radialFormation.Positions;
        GroupRigidbody = transform.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!GameActive && !GroupFormationActive)
            return;
        SetFormation();
    }

    private void FixedUpdate()
    {
        if (!GameActive)
            return;
        if (_spawnedUnitsRigidbodies.Count < 1)
            return;

        if (_joystick.Direction.sqrMagnitude > 0f)
        {
            for (int i = 0; i < _spawnedUnitsRigidbodies.Count; i++)
            {
                float angle = Mathf.Atan2(_joystick.Horizontal, _joystick.Vertical) * Mathf.Rad2Deg;
                _spawnedUnitsRigidbodies[i].transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
            }
        }
        

        if (_joystick.Direction.sqrMagnitude > 0.02f)
        {
            GroupRigidbody.velocity = new Vector3(_joystick.Horizontal * MoveForce, GroupRigidbody.velocity.y,
                _joystick.Vertical * MoveForce);
            for (int i = 0; i < _spawnedUnitsRigidbodies.Count; i++)
            {
                var direction = new Vector3(_joystick.Horizontal * MoveForce, _spawnedUnitsRigidbodies[i].velocity.y,
                    _joystick.Vertical * MoveForce);

                _animatorControllers[i]
                    .ChangeWalkParameter(_joystick.Direction.sqrMagnitude);


                _spawnedUnitsRigidbodies[i].velocity = direction;
                _unitSpeed = firstSpeed * 10f; 
            }

            _returnPos = false;
        }
        else
        {
            for (int i = 0; i < _spawnedUnitsRigidbodies.Count; i++)
            {
                if (!_returnPos)
                {
                    GroupRigidbody.velocity = Vector3.zero;
                    _spawnedUnitsRigidbodies[i].velocity = Vector3.zero;
                }

                _animatorControllers[i]
                    .ChangeWalkParameter(0f);
            }

            if (!_returnPos)
            {
                GroupRigidbody.velocity = Vector3.zero;
            }
            _unitSpeed = firstSpeed; 
            _returnPos = true;
        }
    }

    private void SetFormation()
    {
        if (_spawnedUnits.Count < 1)
            return;
        for (var i = 1; i < _spawnedUnits.Count; i++)
        {
            var dir = ((transform.position + _points[i]) - _spawnedUnits[i].transform.position).normalized;
            dir.y = 0; 
            var rigidbody = _spawnedUnits[i].GetComponent<Rigidbody>();
            if (Vector3.Distance(_spawnedUnits[i].transform.position, transform.position + _points[i]) > .3f)
            {
                _animatorControllers[i]
                    .ChangeWalkParameter(1f);
                if (_returnPos)
                {
                    quaternion = Quaternion.LookRotation(dir);
                    _spawnedUnits[i].transform.rotation = Quaternion.Lerp(_spawnedUnits[i].transform.rotation,
                        quaternion,
                        10f * Time.deltaTime);
                }
                rigidbody.AddForce(dir * _unitSpeed);
            }

            else
            {
                if (_joystick.Direction.sqrMagnitude < 0.02f)
                {
                    _animatorControllers[i]
                        .ChangeWalkParameter(0f);
                }

                rigidbody.velocity = Vector3.zero;
            }
        }

        if (_spawnedUnitsRigidbodies.Count < 1)
            return;
        if (_spawnedUnitsRigidbodies[0] != null)
        {
            var direction = ((transform.position) - _spawnedUnits[0].transform.position).normalized;
            if (Vector3.Distance(_spawnedUnits[0].transform.position, transform.position) > .2f)
            {
                _spawnedUnits[0].transform.rotation = Quaternion.Lerp(_spawnedUnits[0].transform.rotation, quaternion,
                    10f * Time.deltaTime);
                _spawnedUnitsRigidbodies[0].AddForce(direction * _unitSpeed);
                
            }
            else
            {
                _spawnedUnitsRigidbodies[0].velocity = Vector3.zero;
            }
        }
    }

    public void LevelStarted()
    {
        DOVirtual.DelayedCall(.001f,
            () =>
            {
                GameActive.SetValue(true);
                _joystick = GameObject.FindWithTag("Joystick").GetComponent<FloatingJoystick>();
                GroupFormationActive = true;
            });
    }

    [Button]
    public void AddPerson(GameObject Person)
    {
        _spawnedUnits.Add(Person);
        _spawnedUnitsRigidbodies.Add(Person.GetComponent<Rigidbody>());
        _animatorControllers.Add(Person.GetComponent<CharacterAnimatorController>());
        _UnitInsideOfGroup.Add(false);
        _radialFormation.IncreaseAmount(_spawnedUnits.Count);
        _points = _radialFormation.Positions;
        Person.transform.SetParent(_parent);
       
    }

    [Button]
    public void DeactivatePerson(GameObject Person)
    {
        List<bool> boolList = new List<bool>();
        List<GameObject> unitList = new List<GameObject>();
        List<Rigidbody> rigs = new List<Rigidbody>();
        List<CharacterAnimatorController> animatorControllers = new List<CharacterAnimatorController>();
        for (int i = 0; i < _spawnedUnits.Count; i++)
        {
            if (_spawnedUnits[i] != Person)
            {
                boolList.Add(_spawnedUnits[i]);
                unitList.Add(_spawnedUnits[i]);
            }

            if (Person.GetComponent<Rigidbody>() != _spawnedUnitsRigidbodies[i])
            {
                rigs.Add(_spawnedUnitsRigidbodies[i]);
            }

            if (Person.GetComponent<CharacterAnimatorController>() != _animatorControllers[i])
            {
                animatorControllers.Add(_animatorControllers[i]);
            }

            _animatorControllers[i].ChangeWalkParameter(0);
            _spawnedUnitsRigidbodies[i].velocity = Vector3.zero;
        }

        _UnitInsideOfGroup.Clear();
        _UnitInsideOfGroup.AddRange(boolList);
        _spawnedUnitsRigidbodies.Clear();
        _spawnedUnitsRigidbodies.AddRange(rigs);
        _animatorControllers.Clear();
        _animatorControllers.AddRange(animatorControllers);
        _spawnedUnits.Clear();
        _spawnedUnits.AddRange(unitList);
    }

    public void KillPerson(GameObject Person)
    {
        List<bool> boolList = new List<bool>();
        List<GameObject> unitList = new List<GameObject>();
        List<Rigidbody> rigs = new List<Rigidbody>();
        List<CharacterAnimatorController> animatorControllers = new List<CharacterAnimatorController>();
        for (int i = 0; i < _spawnedUnits.Count; i++)
        {
            if (_spawnedUnits[i] != Person)
            {
                boolList.Add(_spawnedUnits[i]);
                unitList.Add(_spawnedUnits[i]);
            }

            if (Person.GetComponent<Rigidbody>() != _spawnedUnitsRigidbodies[i])
            {
                rigs.Add(_spawnedUnitsRigidbodies[i]);
            }

            if (Person.GetComponent<CharacterAnimatorController>() != _animatorControllers[i])
            {
                animatorControllers.Add(_animatorControllers[i]);
            }
        }

        _UnitInsideOfGroup.Clear();
        _UnitInsideOfGroup.AddRange(boolList);
        _spawnedUnitsRigidbodies.Clear();
        _spawnedUnitsRigidbodies.AddRange(rigs);
        _animatorControllers.Clear();
        _animatorControllers.AddRange(animatorControllers);
        _spawnedUnits.Clear();
        _spawnedUnits.AddRange(unitList);
        CurrentWeight.SetValue(CurrentWeight.RuntimeValue - Person.GetComponent<PersonInfo>().GetWeight());
        if (_spawnedUnits.Count == 0)
        {
            _levelManager.LevelFailed();
            GameActive.SetValue(false);
            GroupRigidbody.velocity = Vector3.zero;
            MMVibrationManager.Haptic(HapticTypes.Failure);
        }

    }

    public void LevelCompleted()
    {
        GroupRigidbody.velocity = Vector3.zero;
        foreach (var rig in _spawnedUnitsRigidbodies)
        {
            rig.velocity = Vector3.zero;
        }

        foreach (var animator in _animatorControllers)
        {
            animator.ChangeWalkParameter(0);
        }
    }
}