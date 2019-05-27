using System;
using System.Linq;
using System.Collections.Generic;
using Chisel.Assets;
using Chisel.Core;
using Bounds  = UnityEngine.Bounds;
using Mathf   = UnityEngine.Mathf;
using Vector3 = UnityEngine.Vector3;
using UnitySceneExtensions;

namespace Chisel.Components
{
    // https://www.archdaily.com/892647/how-to-make-calculations-for-staircase-designs
    // https://inspectapedia.com/Stairs/2024s.jpg
    // https://landarchbim.com/2014/11/18/stair-nosing-treads-and-stringers/
    // https://en.wikipedia.org/wiki/Stairs
    [Serializable]
    public struct CSGLinearStairsDefinition
    {
        public enum SurfaceSides
        {
            Top,
            Bottom,
            Left,
            Right,
            Forward,
            Back,
            Tread,
            Step,

            TotalSides
        }

        public const float	kMinStepHeight			= 0.01f;
        public const float	kMinStepDepth			= 0.01f;
        public const float  kMinRiserDepth          = 0.01f;
        public const float  kMinSideWidth			= 0.01f;
        public const float	kMinWidth				= 0.0001f;

        public const float	kDefaultStepHeight		= 0.20f;
        public const float	kDefaultStepDepth		= 0.20f;
        public const float	kDefaultTreadHeight     = 0.02f;
        public const float	kDefaultNosingDepth     = 0.02f; 
        public const float	kDefaultNosingWidth     = 0.01f;

        public const float	kDefaultWidth			= 1;
        public const float	kDefaultHeight			= 1;
        public const float	kDefaultDepth			= 1;

        public const float	kDefaultPlateauHeight	= 0;

        public const float  kDefaultRiserDepth      = 0.03f;
        public const float  kDefaultSideDepth		= 0.0f;
        public const float  kDefaultSideWidth		= 0.03f;
        public const float  kDefaultSideHeight      = 0.03f;

        // TODO: add all spiral stairs improvements to linear stairs
        // TODO: make placement of stairs works properly

        [DistanceValue] public float	stepHeight;
        [DistanceValue] public float	stepDepth;
        [DistanceValue] public float	plateauHeight;
        [DistanceValue] public float	treadHeight;
        [DistanceValue] public float	nosingDepth;
        [DistanceValue] public float	nosingWidth;
        [DistanceValue] public float	riserDepth;
        [DistanceValue] public float	sideDepth;
        [DistanceValue] public float	sideWidth;
        [DistanceValue] public float	sideHeight;
        public Bounds					bounds;
        public StairsRiserType			riserType;
        public StairsSideType           leftSide;
        public StairsSideType           rightSide;
        
        [NonSerialized]
        public ChiselBrushMaterial[]	brushMaterials;
        public SurfaceDescription[]		surfaceDescriptions;
        
        public ChiselBrushMaterial		topSurface;
        public ChiselBrushMaterial		bottomSurface;
        public ChiselBrushMaterial		leftSurface;
        public ChiselBrushMaterial		rightSurface;
        public ChiselBrushMaterial		forwardSurface;
        public ChiselBrushMaterial		backSurface;
        public ChiselBrushMaterial		treadSurface;
        public ChiselBrushMaterial		stepSurface;

        
        public float	width  { get { return bounds.size.x; } set { var size = bounds.size; size.x = value; bounds.size = size; } }
        public float	height { get { return bounds.size.y; } set { var size = bounds.size; size.y = value; bounds.size = size; } }
        public float	depth  { get { return bounds.size.z; } set { var size = bounds.size; size.z = value; bounds.size = size; } }
        public Vector3	size   { get { return bounds.size;   } set { bounds.size = value; } }

        public int StepCount
        {
            get
            {
                const float kSmudgeValue = 0.0001f;
                return Mathf.Max(1,
                          Mathf.FloorToInt((Mathf.Abs(height) - plateauHeight + kSmudgeValue) / stepHeight));
            }
        }

        public float StepDepthOffset
        {
            get { return Mathf.Max(0, Mathf.Abs(depth) - (StepCount * stepDepth)); }
        }

        public void Reset()
        {
            brushMaterials		= null;
            surfaceDescriptions = null;

            // TODO: set defaults using attributes?
            stepHeight		= kDefaultStepHeight;
            stepDepth		= kDefaultStepDepth;
            treadHeight		= kDefaultTreadHeight;
            nosingDepth		= kDefaultNosingDepth;
            nosingWidth		= kDefaultNosingWidth;

            width			= kDefaultWidth;
            height			= kDefaultHeight;
            depth			= kDefaultDepth;

            plateauHeight	= kDefaultPlateauHeight;

            riserType		= StairsRiserType.ThinRiser;
            leftSide		= StairsSideType.None;
            rightSide		= StairsSideType.None;
            riserDepth		= kDefaultRiserDepth;
            sideDepth		= kDefaultSideDepth;
            sideWidth		= kDefaultSideWidth;
            sideHeight		= kDefaultSideHeight;
        }

        public void Validate()
        {
            if (brushMaterials == null ||
                surfaceDescriptions.Length != (int)SurfaceSides.TotalSides)
            {
                var defaultRenderMaterial  = CSGMaterialManager.DefaultWallMaterial;
                var defaultPhysicsMaterial = CSGMaterialManager.DefaultPhysicsMaterial;
                brushMaterials = new ChiselBrushMaterial[(int)SurfaceSides.TotalSides];

                brushMaterials[(int)SurfaceSides.Top    ] = ChiselBrushMaterial.CreateInstance(CSGMaterialManager.DefaultFloorMaterial, defaultPhysicsMaterial);
                brushMaterials[(int)SurfaceSides.Bottom ] = ChiselBrushMaterial.CreateInstance(CSGMaterialManager.DefaultFloorMaterial, defaultPhysicsMaterial);
                brushMaterials[(int)SurfaceSides.Left   ] = ChiselBrushMaterial.CreateInstance(CSGMaterialManager.DefaultWallMaterial,  defaultPhysicsMaterial);
                brushMaterials[(int)SurfaceSides.Right  ] = ChiselBrushMaterial.CreateInstance(CSGMaterialManager.DefaultWallMaterial,  defaultPhysicsMaterial);
                brushMaterials[(int)SurfaceSides.Forward] = ChiselBrushMaterial.CreateInstance(CSGMaterialManager.DefaultWallMaterial,  defaultPhysicsMaterial);
                brushMaterials[(int)SurfaceSides.Back   ] = ChiselBrushMaterial.CreateInstance(CSGMaterialManager.DefaultWallMaterial,  defaultPhysicsMaterial);
                brushMaterials[(int)SurfaceSides.Tread  ] = ChiselBrushMaterial.CreateInstance(CSGMaterialManager.DefaultTreadMaterial, defaultPhysicsMaterial);
                brushMaterials[(int)SurfaceSides.Step   ] = ChiselBrushMaterial.CreateInstance(CSGMaterialManager.DefaultStepMaterial,  defaultPhysicsMaterial);

                for (int i = 0; i < brushMaterials.Length; i++)
                {
                    if (brushMaterials[i] == null)
                        brushMaterials[i] = ChiselBrushMaterial.CreateInstance(defaultRenderMaterial, defaultPhysicsMaterial);
                }
                
                topSurface		= brushMaterials[(int)SurfaceSides.Top    ];
                bottomSurface	= brushMaterials[(int)SurfaceSides.Bottom ];
                leftSurface		= brushMaterials[(int)SurfaceSides.Left   ];
                rightSurface	= brushMaterials[(int)SurfaceSides.Right  ];
                forwardSurface	= brushMaterials[(int)SurfaceSides.Forward];
                backSurface		= brushMaterials[(int)SurfaceSides.Back   ];
                treadSurface	= brushMaterials[(int)SurfaceSides.Tread  ];
                stepSurface		= brushMaterials[(int)SurfaceSides.Step   ];
            }

            if (surfaceDescriptions == null ||
                surfaceDescriptions.Length != 6)
            {
                var surfaceFlags = CSGDefaults.SurfaceFlags;
                surfaceDescriptions = new SurfaceDescription[6];
                for (int i = 0; i < 6; i++)
                {
                    surfaceDescriptions[i] = new SurfaceDescription { surfaceFlags = surfaceFlags, UV0 = UVMatrix.centered };
                }
            }


            stepHeight		= Mathf.Max(kMinStepHeight, stepHeight);
            stepDepth		= Mathf.Clamp(stepDepth, kMinStepDepth, Mathf.Abs(depth));
            treadHeight		= Mathf.Max(0, treadHeight);
            nosingDepth		= Mathf.Max(0, nosingDepth);
            nosingWidth		= Mathf.Max(0, nosingWidth);

            width			= Mathf.Max(kMinWidth, Mathf.Abs(width)) * (width < 0 ? -1 : 1);
            depth			= Mathf.Max(stepDepth, Mathf.Abs(depth)) * (depth < 0 ? -1 : 1);

            riserDepth		= Mathf.Max(kMinRiserDepth, riserDepth);
            sideDepth		= Mathf.Max(0, sideDepth);
            sideWidth		= Mathf.Max(kMinSideWidth, sideWidth);
            sideHeight		= Mathf.Max(0, sideHeight);

            var absHeight        = Mathf.Max(stepHeight, Mathf.Abs(height));
            var maxPlateauHeight = absHeight - stepHeight;

            plateauHeight		= Mathf.Clamp(plateauHeight, 0, maxPlateauHeight);

            var totalSteps		= Mathf.Max(1, Mathf.FloorToInt((absHeight - plateauHeight) / stepHeight));
            var totalStepHeight = totalSteps * stepHeight;

            plateauHeight		= Mathf.Max(0, absHeight - totalStepHeight);
            stepDepth			= Mathf.Clamp(stepDepth, kMinStepDepth, Mathf.Abs(depth) / totalSteps);
        }
    }
}