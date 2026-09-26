package com.monolith.engine3d

import kotlin.math.*

data class Vector3(var x: Float = 0f, var y: Float = 0f, var z: Float = 0f) {
    operator fun plus(v: Vector3) = Vector3(x + v.x, y + v.y, z + v.z)
    operator fun minus(v: Vector3) = Vector3(x - v.x, y - v.y, z - v.z)
    operator fun times(s: Float) = Vector3(x * s, y * s, z * s)
    operator fun div(s: Float) = Vector3(x / s, y / s, z / s)

    fun length() = sqrt(x * x + y * y + z * z)
    fun normalized(): Vector3 {
        val len = length()
        return if (len > 0.00001f) this / len else Vector3()
    }

    fun dot(v: Vector3) = x * v.x + y * v.y + z * v.z
    fun cross(v: Vector3) = Vector3(
        y * v.z - z * v.y,
        z * v.x - x * v.z,
        x * v.y - y * v.x
    )
}

data class Quaternion(var x: Float = 0f, var y: Float = 0f, var z: Float = 0f, var w: Float = 1f) {
    fun toRotationMatrix(): Matrix4x4 {
        val m = Matrix4x4()
        m.m[0] = 1 - 2 * (y * y + z * z)
        m.m[1] = 2 * (x * y - z * w)
        m.m[2] = 2 * (x * z + y * w)

        m.m[4] = 2 * (x * y + z * w)
        m.m[5] = 1 - 2 * (x * x + z * z)
        m.m[6] = 2 * (y * z - x * w)

        m.m[8] = 2 * (x * z - y * w)
        m.m[9] = 2 * (y * z + x * w)
        m.m[10] = 1 - 2 * (x * x + y * y)
        return m
    }
}

class Matrix4x4 {
    val m = FloatArray(16) { 0f }

    init {
        m[0] = 1f; m[5] = 1f; m[10] = 1f; m[15] = 1f
    }

    fun transformPoint(v: Vector3): Vector3 {
        val x = v.x * m[0] + v.y * m[4] + v.z * m[8] + m[12]
        val y = v.x * m[1] + v.y * m[5] + v.z * m[9] + m[13]
        val z = v.x * m[2] + v.y * m[6] + v.z * m[10] + m[14]
        val w = v.x * m[3] + v.y * m[7] + v.z * m[11] + m[15]
        return if (w != 0f) Vector3(x / w, y / w, z / w) else Vector3(x, y, z)
    }

    companion object {
        fun perspective(fovYDeg: Float, aspect: Float, zNear: Float, zFar: Float): Matrix4x4 {
            val mat = Matrix4x4()
            val f = 1f / tan(Math.toRadians(fovYDeg.toDouble() / 2.0)).toFloat()
            mat.m[0] = f / aspect
            mat.m[5] = f
            mat.m[10] = (zFar + zNear) / (zNear - zFar)
            mat.m[11] = -1f
            mat.m[14] = (2f * zFar * zNear) / (zNear - zFar)
            mat.m[15] = 0f
            return mat
        }

        fun lookAt(eye: Vector3, center: Vector3, up: Vector3): Matrix4x4 {
            val f = (center - eye).normalized()
            val s = f.cross(up).normalized()
            val u = s.cross(f)

            val mat = Matrix4x4()
            mat.m[0] = s.x; mat.m[4] = s.y; mat.m[8] = s.z
            mat.m[1] = u.x; mat.m[5] = u.y; mat.m[9] = u.z
            mat.m[2] = -f.x; mat.m[6] = -f.y; mat.m[10] = -f.z
            mat.m[12] = -s.dot(eye); mat.m[13] = -u.dot(eye); mat.m[14] = f.dot(eye)
            return mat
        }
    }
}

data class Mesh3D(val vertices: List<Vector3>, val triangles: List<IntArray>, val normals: List<Vector3>)
