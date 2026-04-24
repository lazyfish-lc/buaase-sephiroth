using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class NumericalRuleTests {
    private const float Tolerance = 0.0001f;

    private readonly List<GameObject> createdObjects = new List<GameObject>();
    private readonly List<Object> createdAssets = new List<Object>();
    private readonly List<string> registeredBigObjectNames = new List<string>();

    [TearDown]
    public void TearDown() {
        foreach (string bigObjectName in registeredBigObjectNames) {
            GameObjectManager.UnregisterBigObject(bigObjectName);
        }

        for (int i = createdObjects.Count - 1; i >= 0; i--) {
            Object.DestroyImmediate(createdObjects[i]);
        }

        for (int i = createdAssets.Count - 1; i >= 0; i--) {
            Object.DestroyImmediate(createdAssets[i]);
        }

        registeredBigObjectNames.Clear();
        createdObjects.Clear();
        createdAssets.Clear();
    }

    [Test]
    public void SmallObjectPropertySetValue_WhenValueIsValid_UpdatesValueAndNotifiesRules() {
        SmallObjectProperty property = new SmallObjectProperty("energy", 5f, 0f, 10f);
        RecordingRule rule = new RecordingRule();
        Assert.That(property.AddNumericalRule(rule), Is.True);

        bool result = property.SetValue(7f);

        Assert.That(result, Is.True);
        Assert.That(property.value, Is.EqualTo(7f).Within(Tolerance));
        Assert.That(rule.CheckValidCallCount, Is.EqualTo(1));
        Assert.That(rule.NotifyCallCount, Is.EqualTo(1));
        Assert.That(rule.LastCheckOldValue, Is.EqualTo(5f).Within(Tolerance));
        Assert.That(rule.LastCheckNewValue, Is.EqualTo(7f).Within(Tolerance));
        Assert.That(rule.LastNotifyOldValue, Is.EqualTo(5f).Within(Tolerance));
        Assert.That(rule.LastNotifyNewValue, Is.EqualTo(7f).Within(Tolerance));
    }

    [Test]
    public void SmallObjectPropertySetValue_WhenValueIsOutOfRange_RejectsBeforeCheckingRules() {
        SmallObjectProperty property = new SmallObjectProperty("energy", 5f, 0f, 10f);
        RecordingRule rule = new RecordingRule();
        Assert.That(property.AddNumericalRule(rule), Is.True);

        bool result = property.SetValue(11f);

        Assert.That(result, Is.False);
        Assert.That(property.value, Is.EqualTo(5f).Within(Tolerance));
        Assert.That(rule.CheckValidCallCount, Is.EqualTo(0));
        Assert.That(rule.NotifyCallCount, Is.EqualTo(0));
    }

    [Test]
    public void SmallObjectPropertySetValue_WhenRuleRejects_LeavesValueAndDoesNotNotify() {
        SmallObjectProperty property = new SmallObjectProperty("energy", 5f, 0f, 10f);
        RecordingRule rule = new RecordingRule { IsValid = false };
        Assert.That(property.AddNumericalRule(rule), Is.True);

        bool result = property.SetValue(7f);

        Assert.That(result, Is.False);
        Assert.That(property.value, Is.EqualTo(5f).Within(Tolerance));
        Assert.That(rule.CheckValidCallCount, Is.EqualTo(1));
        Assert.That(rule.NotifyCallCount, Is.EqualTo(0));
    }

    [Test]
    public void MaintainConstantRuleNotify_DistributesInputDeltaAcrossOutputsByWeight() {
        SmallObjectProperty input = new SmallObjectProperty("input", 10f, 0f, 100f);
        SmallObjectProperty outputA = new SmallObjectProperty("outputA", 20f, 0f, 100f);
        SmallObjectProperty outputB = new SmallObjectProperty("outputB", 30f, 0f, 100f);
        RecordingRule outputRule = new RecordingRule();
        Assert.That(outputA.AddNumericalRule(outputRule), Is.True);

        NumericalMaintainConstantRule rule = new NumericalMaintainConstantRule {
            inputProperty = input,
            inputWeight = 2f,
            outputProperties = new List<SmallObjectProperty> { outputA, outputB },
            outputWeights = new List<float> { 1f, 3f }
        };

        rule.Notify(10f, 14f);

        Assert.That(outputA.value, Is.EqualTo(18f).Within(Tolerance));
        Assert.That(outputB.value, Is.EqualTo(24f).Within(Tolerance));
        Assert.That(outputRule.NotifyCallCount, Is.EqualTo(0));
    }

    [Test]
    public void MaintainConstantRuleNotify_WhenOutputWouldExceedBounds_DoesNotModifyOutputs() {
        SmallObjectProperty outputA = new SmallObjectProperty("outputA", 2f, 0f, 10f);
        SmallObjectProperty outputB = new SmallObjectProperty("outputB", 8f, 0f, 10f);

        NumericalMaintainConstantRule rule = new NumericalMaintainConstantRule {
            inputWeight = 1f,
            outputProperties = new List<SmallObjectProperty> { outputA, outputB },
            outputWeights = new List<float> { 1f, 1f }
        };

        rule.Notify(0f, 20f);

        Assert.That(outputA.value, Is.EqualTo(2f).Within(Tolerance));
        Assert.That(outputB.value, Is.EqualTo(8f).Within(Tolerance));
    }

    [Test]
    public void MaintainConstantRuleNotify_WhenOutputWeightsSumToZero_DoesNotModifyOutputs() {
        SmallObjectProperty outputA = new SmallObjectProperty("outputA", 20f, 0f, 100f);
        SmallObjectProperty outputB = new SmallObjectProperty("outputB", 30f, 0f, 100f);

        NumericalMaintainConstantRule rule = new NumericalMaintainConstantRule {
            inputWeight = 1f,
            outputProperties = new List<SmallObjectProperty> { outputA, outputB },
            outputWeights = new List<float> { 1f, -1f }
        };

        rule.Notify(10f, 14f);

        Assert.That(outputA.value, Is.EqualTo(20f).Within(Tolerance));
        Assert.That(outputB.value, Is.EqualTo(30f).Within(Tolerance));
    }

    [Test]
    public void NumericalRuleFactoryBuildMaintainConstantRule_CreatesRuleForEachTermAndAttachesInputs() {
        PropertyGraph graph = CreateRegisteredPropertyGraph();
        string ruleString = $"2*{graph.BigObjectName}.Past.energy + 3*{graph.BigObjectName}.Present.mass + 5*{graph.BigObjectName}.Present.heat = 100";

        bool result = NumericalRuleFactory.BuildMaintainConstantRule(ruleString, out List<NumericalMaintainConstantRule> rules);

        Assert.That(result, Is.True);
        Assert.That(rules, Has.Count.EqualTo(3));
        Assert.That(rules[0].inputProperty, Is.SameAs(graph.Energy));
        Assert.That(rules[0].inputWeight, Is.EqualTo(2f).Within(Tolerance));
        Assert.That(rules[0].outputProperties[0], Is.SameAs(graph.Mass));
        Assert.That(rules[0].outputProperties[1], Is.SameAs(graph.Heat));
        Assert.That(rules[0].outputWeights[0], Is.EqualTo(3f).Within(Tolerance));
        Assert.That(rules[0].outputWeights[1], Is.EqualTo(5f).Within(Tolerance));

        Assert.That(graph.Energy.SetValue(12f), Is.True);
        Assert.That(graph.Mass.value, Is.EqualTo(18.5f).Within(Tolerance));
        Assert.That(graph.Heat.value, Is.EqualTo(27.5f).Within(Tolerance));
    }

    [Test]
    public void NumericalRuleFactoryBuildRule_WhenMaintainRuleIsValid_ReturnsBaseRuleList() {
        PropertyGraph graph = CreateRegisteredPropertyGraph();
        string ruleString = $"1*{graph.BigObjectName}.Past.energy + 1*{graph.BigObjectName}.Present.mass = 30";

        bool result = NumericalRuleFactory.BuildRule(ruleString, out List<NumericalRule> rules);

        Assert.That(result, Is.True);
        Assert.That(rules, Has.Count.EqualTo(2));
        Assert.That(rules[0], Is.TypeOf<NumericalMaintainConstantRule>());
        Assert.That(rules[1], Is.TypeOf<NumericalMaintainConstantRule>());
    }

    [TestCase("")]
    [TestCase("1*Missing.Past.energy")]
    [TestCase("1*Missing.Past.energy = 10")]
    public void NumericalRuleFactoryBuildRule_WhenExpressionIsInvalid_ReturnsFalseAndNoRules(string ruleString) {
        bool result = NumericalRuleFactory.BuildRule(ruleString, out List<NumericalRule> rules);

        Assert.That(result, Is.False);
        Assert.That(rules, Is.Empty);
    }

    [Test]
    public void NumericalRuleManagerRegisterRule_AttachesRuleToInputProperty() {
        SmallObjectProperty property = new SmallObjectProperty("energy", 5f, 0f, 10f);
        RecordingRule rule = new RecordingRule { inputProperty = property };

        NumericalRuleManager.RegisterRule(rule);

        Assert.That(property.SetValue(6f), Is.True);
        Assert.That(rule.NotifyCallCount, Is.EqualTo(1));
        Assert.That(rule.LastNotifyOldValue, Is.EqualTo(5f).Within(Tolerance));
        Assert.That(rule.LastNotifyNewValue, Is.EqualTo(6f).Within(Tolerance));
    }

    [Test]
    public void NumericalRuleManagerRegisterRules_AttachesEveryRuleToItsInputProperty() {
        SmallObjectProperty firstProperty = new SmallObjectProperty("first", 1f, 0f, 10f);
        SmallObjectProperty secondProperty = new SmallObjectProperty("second", 2f, 0f, 10f);
        RecordingRule firstRule = new RecordingRule { inputProperty = firstProperty };
        RecordingRule secondRule = new RecordingRule { inputProperty = secondProperty };

        NumericalRuleManager.RegisterRules(new List<NumericalRule> { firstRule, secondRule });

        Assert.That(firstProperty.SetValue(3f), Is.True);
        Assert.That(secondProperty.SetValue(4f), Is.True);
        Assert.That(firstRule.NotifyCallCount, Is.EqualTo(1));
        Assert.That(secondRule.NotifyCallCount, Is.EqualTo(1));
    }

    [Test]
    public void NumericalRuleManagerBuildAndRegisterRules_WithValidRuleString_AttachesBuiltRules() {
        PropertyGraph graph = CreateRegisteredPropertyGraph();
        string ruleString = $"1*{graph.BigObjectName}.Past.energy + 1*{graph.BigObjectName}.Present.mass = 30";

        NumericalRuleManager.BuildAndRegisterRules(ruleString);

        Assert.That(graph.Energy.SetValue(15f), Is.True);
        Assert.That(graph.Mass.value, Is.EqualTo(15f).Within(Tolerance));
    }

    [Test]
    public void NumericalRuleManagerGuardClauses_DoNotThrowForInvalidInputs() {
        Assert.DoesNotThrow(() => NumericalRuleManager.RegisterRule(null));
        Assert.DoesNotThrow(() => NumericalRuleManager.RegisterRule(new NumericalRule()));
        Assert.DoesNotThrow(() => NumericalRuleManager.RegisterRules(null));
        Assert.DoesNotThrow(() => NumericalRuleManager.RegisterRules(new List<NumericalRule>()));
        Assert.DoesNotThrow(() => NumericalRuleManager.BuildAndRegisterRules(null));
        Assert.DoesNotThrow(() => NumericalRuleManager.BuildAndRegisterRules(""));
    }

    private PropertyGraph CreateRegisteredPropertyGraph() {
        string bigObjectName = "Big_" + Guid.NewGuid().ToString("N");
        EnvironmentBigObject bigObject = CreateBigObject(bigObjectName);
        DoorSmallObject pastObject = CreateSmallObject("Past");
        DoorSmallObject presentObject = CreateSmallObject("Present");

        bigObject.staticData.pastObject = pastObject;
        bigObject.staticData.presentObject = presentObject;
        GameObjectManager.RegisterBigObject(bigObjectName, bigObject);
        registeredBigObjectNames.Add(bigObjectName);

        return new PropertyGraph {
            BigObjectName = bigObjectName,
            Energy = AddProperty(pastObject, "energy", 10f, 0f, 100f),
            Mass = AddProperty(presentObject, "mass", 20f, 0f, 100f),
            Heat = AddProperty(presentObject, "heat", 30f, 0f, 100f)
        };
    }

    private EnvironmentBigObject CreateBigObject(string objectName) {
        GameObject gameObject = new GameObject(objectName);
        createdObjects.Add(gameObject);

        EnvironmentBigObject bigObject = gameObject.AddComponent<EnvironmentBigObject>();
        bigObject.staticData = ScriptableObject.CreateInstance<BigObjectStaticData>();
        createdAssets.Add(bigObject.staticData);
        return bigObject;
    }

    private DoorSmallObject CreateSmallObject(string objectName) {
        GameObject gameObject = new GameObject(objectName);
        createdObjects.Add(gameObject);

        DoorSmallObject smallObject = gameObject.AddComponent<DoorSmallObject>();
        smallObject.staticData = ScriptableObject.CreateInstance<SmallObjectStaticData>();
        createdAssets.Add(smallObject.staticData);
        smallObject.staticData.objectName = objectName;
        smallObject.dynamicState = new SmallObjectDynamicState();
        return smallObject;
    }

    private static SmallObjectProperty AddProperty(
        SmallObject smallObject,
        string propertyName,
        float value,
        float minValue,
        float maxValue
    ) {
        SmallObjectProperty property = new SmallObjectProperty(propertyName, value, minValue, maxValue);
        smallObject.dynamicState.propertyMap[propertyName] = property;
        return property;
    }

    private sealed class RecordingRule : NumericalRule {
        public bool IsValid = true;
        public int CheckValidCallCount;
        public int NotifyCallCount;
        public float LastCheckOldValue;
        public float LastCheckNewValue;
        public float LastNotifyOldValue;
        public float LastNotifyNewValue;

        public override bool CheckValid(float oldValue, float newValue) {
            CheckValidCallCount++;
            LastCheckOldValue = oldValue;
            LastCheckNewValue = newValue;
            return IsValid;
        }

        public override void Notify(float oldValue, float newValue) {
            NotifyCallCount++;
            LastNotifyOldValue = oldValue;
            LastNotifyNewValue = newValue;
        }
    }

    private sealed class PropertyGraph {
        public string BigObjectName;
        public SmallObjectProperty Energy;
        public SmallObjectProperty Mass;
        public SmallObjectProperty Heat;
    }
}
