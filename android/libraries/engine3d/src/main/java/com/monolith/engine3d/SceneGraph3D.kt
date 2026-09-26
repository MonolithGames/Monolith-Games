package com.monolith.engine3d

class SceneNode3D(val name: String) {
    var position: Vector3 = Vector3()
    var rotation: Quaternion = Quaternion()
    var scale: Vector3 = Vector3(1f, 1f, 1f)
    val children: MutableList<SceneNode3D> = mutableListOf()
    var mesh: Mesh3D? = null

    fun addChild(child: SceneNode3D) {
        children.add(child)
    }

    fun computeWorldMatrix(): Matrix4x4 {
        val mat = rotation.toRotationMatrix()
        mat.m[12] = position.x
        mat.m[13] = position.y
        mat.m[14] = position.z
        return mat
    }
}

class SceneGraph3D {
    val root: SceneNode3D = SceneNode3D("RootNode")

    fun render(camera: Matrix4x4) {
        traverse(root, Matrix4x4())
    }

    private fun traverse(node: SceneNode3D, parentMatrix: Matrix4x4) {
        val worldMat = node.computeWorldMatrix()
        node.mesh?.let { m ->
            // Rasterize mesh triangles into framebuffer
        }
        for (child in node.children) {
            traverse(child, worldMat)
        }
    }
}
