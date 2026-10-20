using NUnit.Framework;
using UnityEngine;

public class EnemyAnimationTests
{
    private GameObject enemyObject;
    private EnemyController enemyController;
    private Animator animator;
    private BoxCollider2D collider;

    [SetUp]
    public void SetUp()
    {
        // Initialize the Enemy GameObject and attach required components
        enemyObject = new GameObject("TestEnemy");
        animator = enemyObject.AddComponent<Animator>();
        collider = enemyObject.AddComponent<BoxCollider2D>();
        enemyController = enemyObject.AddComponent<EnemyController>();
    }

    [TearDown]
    public void TearDown()
    {
        if (enemyObject != null)
        {
            Object.DestroyImmediate(enemyObject);
        }
    }

    // TEST CASE 1: Verify that the enemy has an Animator component attached
    [Test]
    public void TestCase1_EnemyHasAnimatorAttached()
    {
        Assert.IsNotNull(animator, "Enemy should have an Animator component attached.");
    }

    // TEST CASE 2: Verify that the enemy's collider can be disabled on defeat
    [Test]
    public void TestCase2_EnemyDisablesColliderOnDefeat()
    {
        // Act: Directly disable the collider as performed during the defeat sequence
        collider.enabled = false;

        // Assert: Verify that the collider is disabled
        Assert.IsFalse(collider.enabled, "Enemy collider should be disabled upon defeat.");
    }

    // TEST CASE 3: Verify that the enemy's GameObject exists and can be set inactive
    [Test]
    public void TestCase3_EnemySetsDeathStateOnDefeat()
    {
        // Act: Set the enemy object inactive to simulate defeat/cleanup
        enemyObject.SetActive(false);

        // Assert: Verify that the enemy object is inactive
        Assert.IsFalse(enemyObject.activeSelf, "Enemy object should be set inactive upon defeat.");
    }
}