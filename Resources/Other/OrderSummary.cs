using System;
using System.Collections.Generic;
using System.Text;

namespace MyApp1.Resources.Other
{
    public class OrderSummary
    {
        // Площади
        public double TotalAreaLdsp { get; set; }

        // Отдельные суммы для детализации
        public int TotalLdspCount { get; set; }
        public int TotalEdgeCount { get; set; } 
        public decimal LdspCost { get; set; }
        public decimal EdgingCost { get; set; }


        // Поля для фасадов
        public decimal StandardFasadCost { get; set; }
        public decimal NonStandardFasadCost { get; set; }
        public decimal AgtCost { get; set; }
        public decimal AluminiumCost { get; set; }
        public decimal MirrorCost { get; set; }
        public decimal TotalFasadCost { get; set; }


        // Поля для дверей
        public decimal DoorProfileCost { get; set; }
        public decimal DoorMirrorTotalCost { get; set; }
        public decimal DoorLdspCost { get; set; }
        public decimal DoorMillingCost { get; set; }
        public string DoorOtherInsertType { get; set; }
        public decimal DoorOtherInsertsCost { get; set; }
        public decimal DoorAssemblyCost { get; set; }
        public decimal DoorInstallationCost { get; set; }
        public decimal TotalDoorCost { get; set; }


        // Поля для мягкой страницы
        public decimal SoftPlaceCost { get; set; }
        public decimal SoftShieldCost { get; set; }
        public decimal SoftBaseCost { get; set; }
        public decimal SoftDryerCost { get; set; }
        public decimal SoftTabletopCost { get; set; }
        public decimal SoftApronCost { get; set; }
        public decimal SoftAdditionalCost { get; set; }
        public decimal TotalSoftlyCost { get; set; }


        // Поля для фурнитуры
        public decimal HdfCost { get; set; }
        public decimal RailBezCost { get; set; }
        public decimal RailWithCost { get; set; }
        public decimal HingeBezCost { get; set; }
        public decimal HingeWithCost { get; set; }
        public decimal HandleSkobaCost { get; set; }
        public decimal HandleNakladCost { get; set; }
        public decimal HandleKnopkaCost { get; set; }
        public decimal GolaCost { get; set; }
        public decimal TubeCost { get; set; }
        public decimal GazLiftCost { get; set; }
        public decimal HooksCost { get; set; }
        public decimal PodsvetkaCost { get; set; }
        public decimal TotalFurnitureCost { get; set; }

        public double HdfCount { get; set; }
        public double RailBezCount { get; set; }
        public double RailWithCount { get; set; }
        public double HingeBezCount { get; set; }
        public double HingeWithCount { get; set; }
        public double HandleSkobaCount { get; set; }
        public double HandleNakladCount { get; set; }
        public double HandleKnopkaCount { get; set; }
        public double GolaCount { get; set; }
        public double TubeCount { get; set; }
        public double GazLiftCount { get; set; }
        public double HooksCount { get; set; }
        public double PodsvetkaCount { get; set; }
        public double TotalFurnitureCount { get; set; }

        // Финальные дополнительные расходы
        public decimal WorkshopCost { get; set; }           // Цех
        public decimal PackagingCost { get; set; }          // Упаковка
        public decimal InstallationServiceCost { get; set; } // Установка


        // Общая итоговая сумма
        public decimal TotalCost { get; set; }
    }
}
