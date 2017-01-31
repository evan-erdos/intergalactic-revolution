/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-08-22 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Inventories {
    public class Gun : Weapon {
        public bool isAuto, isBurst, isPrimary;
        public uint
            countAmmo = 24, sizeClip = 7, countLoaded = 7,
            countBursts = 1, fovScoped = 30, range = 128;
        public float
            rateSpread = 0.1f, rateCool = 0.05f, rateScope = 0.06f, rateMax = 0.3f,
            spread = 0.1f, spreadAimed = 0.1f, spreadMax = 0.1f, spreadJam = 1,
            timeShot = 0.05f, sinceShot = 0, timeReload = 2, sinceReload = 0,
            forceShot = 443, damageShot = 1024, deltaShot = 256;
        public string MunitionType = "9mm PARA";
        public string[] animNames, animFire, animReload;
        public enum WeaponTypes { Projectile, Ballistic, Crystal, Melee };
        public WeaponTypes WeaponType = WeaponTypes.Projectile;
        public AudioClip audEmpty;
        public AudioClip[] audImpacts, audReloads;
        public Transform[] trPos;
        public GameObject fireShell, fireFlash, fireExplosion;
        public GameObject[] ShotParticles;

        void Update() {
            if (sinceShot<Rate) sinceShot += Time.deltaTime;
            if (sinceReload<timeReload) sinceReload += Time.deltaTime;
            if (Time.timeScale>0) {
                if (Input.GetButtonDown("Reload")) Reload();
                if (Input.GetMouseButtonDown(0)) Use();
            }
        }

        public override void Attack() {
            var direction = Spray(transform.forward,spread);
            if (!Physics.Raycast(
                origin: transform.position,
                direction: direction,
                hitInfo: out RaycastHit hit,
                maxDistance: range,
                layerMask: Mask)) return;
            var normal = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Instantiate(
                ShotParticles[Random.Range(0,ShotParticles.Length)],
                hit.point,
                normal);
            hit.rigidbody?.AddForceAtPosition(
                force: forceShot*direction,
                position: hit.point);
        }

        internal void Reload() {
            if (countLoaded>=sizeClip || sinceReload<=timeReload) return;
            if (countAmmo>(sizeClip-countLoaded))
                countAmmo -= sizeClip-countLoaded;
            else if (countAmmo < (sizeClip-countLoaded))
                countLoaded = countAmmo;
            countAmmo = countAmmo-countLoaded;
            sinceReload = 0;
        }

        static Vector3 Spray(Vector3 direction, float spread) {
            var splay = new Vector2(direction.x, direction.y);
            var deviation = Random.insideUnitCircle;
            deviation -= new Vector2(0.5f, 0.5f);
            splay += deviation * spread;
            direction.x += splay.x;
            direction.y += splay.y;
            return direction;
        }
    }
}
