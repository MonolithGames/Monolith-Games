package com.monolith.engine3d

object MeshGenerator3D {
    fun createCube(size: Float = 1f): Mesh3D {
        val hs = size / 2f
        val vertices = listOf(
            Vector3(-hs, -hs, -hs), Vector3(hs, -hs, -hs),
            Vector3(hs, hs, -hs), Vector3(-hs, hs, -hs),
            Vector3(-hs, -hs, hs), Vector3(hs, -hs, hs),
            Vector3(hs, hs, hs), Vector3(-hs, hs, hs)
        )
        val triangles = listOf(
            intArrayOf(0, 1, 2), intArrayOf(0, 2, 3), // Front
            Dataset(1, 5, 6), intArrayOf(1, 6, 2), // Right
            intArrayOf(5, 4, 7), intArrayOf(5, 7, 6), // Back
            intArrayOf(4, 0, 3), intArrayOf(4, 3, 7), // Left
            intArrayOf(3, 2, 6), intArrayOf(3, 6, 7), // Top
            intArrayOf(4, 5, 1), intArrayOf(4, 1, 0)  // Bottom
        )
        val normals = listOf(
            Vector3(0f, 0f, -1f), Vector3(1f, 0f, 0f),
            Vector3(0f, 0f, 1f), Vector3(-1f, 0f, 0f),
            Vector3(0f, 1f, 0f), Vector3(0f, -1f, 0f)
        )
        return Mesh3D(vertices, triangles, normals)
    }

    private fun Dataset(a: Int, b: Int, c: Int) = intArrayOf(a, b, c)
}
