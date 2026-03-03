using MyApp1.Resources.Other;
using System;
using System.Linq;

namespace MyApp1;

public static class CalculatorService
{
    public static OrderSummary Calculate(ObjectData order, PriceList prices)
    {
        var summary = new OrderSummary();

        // --- . ПОДГОТОВКА ПЛОЩАДЕЙ ---

        // Площадь обычных деталей ЛДСП (полки, перегородки и т.д.)
        double baseLdspArea = order.LdspForms.Sum(f => f.Area);

        // ПЛОЩАДЬ ФАСАДОВ ИЗ ЛДСП (добавляем к общему расходу материала)
        double ldspFasadArea = order.SpecialFasads
            .Where(f => f.SelectedType == FasadType.LDSP)
            .Sum(f => f.Area);

        // СУММАРНАЯ ПЛОЩАДЬ ЛДСП (Материал + Фасады из ЛДСП)
        double totalLdspArea = baseLdspArea + ldspFasadArea;

        summary.TotalAreaLdsp = totalLdspArea;

        // --- 1. РАСЧЕТ ЛДСП И КРОМКИ ---
        // (Используем 5.0 как площадь листа, 20 - коэф. кромки на м2)
        int sheetsCount = (int)Math.Ceiling(totalLdspArea / 5.0);
        int edgeCount = (int)(sheetsCount * 20);

        summary.TotalEdgeCount = edgeCount;
        summary.TotalLdspCount = sheetsCount;
        summary.LdspCost = sheetsCount * prices.Ldsp;
        summary.EdgingCost = sheetsCount * 20 * prices.Edging;

        // --- 2. РАСЧЕТ ФАСАДОВ ---

        // А. Стандартные фасады
        double standardArea = order.StandardFasads.Sum(f => f.Area);
        summary.StandardFasadCost = (decimal)standardArea * prices.Film;

        // Б. Нестандартные фасады (+25%)
        double nonStandardArea = order.NonStandardFasads.Sum(f => f.Area);
        decimal baseNonStandardCost = (decimal)nonStandardArea * prices.Film;
        summary.NonStandardFasadCost = baseNonStandardCost * 1.25m;

        // В. Специальные фасады

        // AGT
        double agtArea = order.SpecialFasads
            .Where(f => f.SelectedType == FasadType.AGT)
            .Sum(f => f.Area);
        summary.AgtCost = (decimal)agtArea * prices.Agt;

        // Aluminium
        double alumArea = order.SpecialFasads
            .Where(f => f.SelectedType == FasadType.Aluminium)
            .Sum(f => f.Area);
        summary.AluminiumCost = (decimal)alumArea * prices.Aluminum;

        // Mirror
        double mirrorArea = order.SpecialFasads
            .Where(f => f.SelectedType == FasadType.Mirror)
            .Sum(f => f.Area);
        summary.MirrorCost = (decimal)mirrorArea * prices.DoorMirror;

        summary.TotalFasadCost = summary.StandardFasadCost + summary.NonStandardFasadCost +
                            summary.AgtCost + summary.AluminiumCost + summary.MirrorCost;

        // --- 3. РАСЧЕТ ДВЕРЕЙ КУПЕ ---
        var d = order.DoorData;

        summary.DoorProfileCost = d.DoorCount * 2 * prices.DoorProfile;
        summary.DoorMirrorTotalCost = (decimal)d.Mirror.Area * 2 * prices.DoorMirror;

        double ldspInsertArea = d.Ldsp.Area;
        if (ldspInsertArea == 0)
            summary.DoorLdspCost = 0;
        else if (ldspInsertArea < 5)
            summary.DoorLdspCost = 2 * prices.DoorLdsp;
        else
            summary.DoorLdspCost = 4 * prices.DoorLdsp;

        summary.DoorMillingCost = (decimal)d.Router.Area * prices.Milling;

        // --- ИСПРАВЛЕНО: Switch по Enum для вставок двери ---
        summary.DoorOtherInsertType = d.Other.SelectedType.ToString(); // Конвертируем в строку для отчета

        decimal otherPrice = d.Other.SelectedType switch
        {
            OtherInsertType.MDF => prices.Mdf,
            OtherInsertType.Lacobel => prices.Lacobel,
            OtherInsertType.GraphiteMirror => prices.GraphiteMirror,
            OtherInsertType.MatteGlass => prices.FrostedGlass,
            OtherInsertType.GraphiteGlass => prices.GraphiteGlass,
            _ => 0
        };
        summary.DoorOtherInsertsCost = (decimal)d.Other.Area * 2 * otherPrice;

        summary.DoorAssemblyCost = d.DoorCount * 600;

        decimal doorSubTotal = summary.DoorProfileCost + summary.DoorMirrorTotalCost +
                               summary.DoorLdspCost + summary.DoorMillingCost +
                               summary.DoorOtherInsertsCost + summary.DoorAssemblyCost;

        summary.DoorInstallationCost = doorSubTotal * 0.10m;
        summary.TotalDoorCost = doorSubTotal + summary.DoorInstallationCost;

        // --- 4. РАСЧЕТ МЯГКОЙ СТРАНИЦЫ ---
        var s = order.SoftlyData;

        // --- ИСПРАВЛЕНО: Switch по Enum для категорий ---
        summary.SoftPlaceCost = s.SelectedCategory switch
        {
            SoftCategory.Standard => prices.SoftPlace,
            SoftCategory.Peretyazhka => prices.Stretching,
            SoftCategory.Karetka => prices.CarriageCoupler,
            _ => 0 // SoftCategory.None
        };

        // --- ИСПРАВЛЕНО: Switch по Enum для типа щита ---
        decimal softShieldPrice = s.SelectedShieldType switch
        {
            SoftShieldType.Peretyazhka => prices.SoftShieldBinding,
            SoftShieldType.Karetka => prices.SoftShieldCarriage,
            _ => 0
        };
        summary.SoftShieldCost = (decimal)s.Area * softShieldPrice;

        summary.SoftBaseCost = s.HasBase ? prices.BasePrice : 0;
        summary.SoftDryerCost = s.HasDryer ? prices.DryerPrice : 0;

        summary.SoftTabletopCost = s.Tabletop ?? 0;
        summary.SoftApronCost = s.Apron ?? 0;
        summary.SoftAdditionalCost = s.Additional ?? 0;

        summary.TotalSoftlyCost = summary.SoftPlaceCost + summary.SoftShieldCost +
                                  summary.SoftBaseCost + summary.SoftDryerCost +
                                  summary.SoftTabletopCost + summary.SoftApronCost +
                                  summary.SoftAdditionalCost;

        // --- 5. РАСЧЕТ ФУРНИТУРЫ ---
        var f = order.Furniture;

        summary.HdfCost = (decimal)(f.Hdf ?? 0) * prices.Hdf;
        summary.RailBezCost = (decimal)(f.NaprBez ?? 0) * prices.RailNoCloser;
        summary.RailWithCost = (decimal)(f.NaprS ?? 0) * prices.RailWithCloser;
        summary.HingeBezCost = (decimal)(f.PetliBez ?? 0) * prices.HingeNoCloser;
        summary.HingeWithCost = (decimal)(f.PetliS ?? 0) * prices.HingeWithCloser;
        summary.HandleSkobaCost = (decimal)(f.Skoba ?? 0) * prices.HandleBracket;
        summary.HandleNakladCost = (decimal)(f.Nakladnaya ?? 0) * prices.HandleOverhead;
        summary.HandleKnopkaCost = (decimal)(f.Knopka ?? 0) * prices.HandleKnob;
        summary.GolaCost = (decimal)(f.Gola ?? 0) * prices.ProfileGola;
        summary.TubeCost = (decimal)(f.Truba ?? 0) * prices.Tube;
        summary.GazLiftCost = (decimal)(f.GazLift ?? 0) * prices.GasLift;
        summary.HooksCost = (decimal)(f.Kruchki ?? 0) * prices.Hook;
        summary.PodsvetkaCost = (decimal)(f.Podsvetka ?? 0) * prices.Lighting;
        summary.HdfCount =f.Hdf??0;
        summary.RailBezCount = f.NaprBez ?? 0;
        summary.RailWithCount = f.NaprS ?? 0;
        summary.HingeBezCount =f.PetliBez ?? 0;
        summary.HingeWithCount =f.PetliS ?? 0;
        summary.HandleSkobaCount = f.Skoba ?? 0;
        summary.HandleNakladCount = f.Nakladnaya ?? 0;
        summary.HandleKnopkaCount = f.Knopka ?? 0;
        summary.GolaCount = f.Gola ?? 0;
        summary.TubeCount =f.Truba ?? 0;
        summary.GazLiftCount = f.GazLift ?? 0;
        summary.HooksCount = f.Kruchki ?? 0;
        summary.PodsvetkaCount = f.Podsvetka ?? 0;

        summary.TotalFurnitureCost = summary.HdfCost + summary.RailBezCost + summary.RailWithCost +
                                     summary.HingeBezCost + summary.HingeWithCost + summary.HandleSkobaCost +
                                     summary.HandleNakladCost + summary.HandleKnopkaCost + summary.GolaCost +
                                     summary.TubeCost + summary.GazLiftCost + summary.HooksCost +
                                     summary.PodsvetkaCost;

        // --- 6. ФИНАЛЬНЫЕ РАСЧЕТЫ ---

        // ЦЕХ: Пила + Кромление + Присадка + 10% от мягких работ + Присадка петель
        summary.WorkshopCost = (sheetsCount * prices.Sawing) +
                               (sheetsCount * 20 * prices.EdgingService) +
                               ((decimal)totalLdspArea * prices.Additive) +
                               ((summary.SoftPlaceCost + summary.SoftShieldCost) * 0.1m) +
                               ((decimal)((f.PetliBez ?? 0) + (f.PetliS ?? 0)) * prices.HingeAdditive);

        // УПАКОВКА: Суммарная площадь всех фасадов * цену упаковки
        double totalFasadArea = standardArea + nonStandardArea + agtArea + alumArea;
        summary.PackagingCost = (decimal)totalFasadArea * prices.Packaging;

        // СУММА МАТЕРИАЛОВ (без дверей)
        decimal materialsBase = summary.LdspCost + summary.EdgingCost + summary.TotalFasadCost +
                                summary.TotalSoftlyCost + summary.TotalFurnitureCost;

        // УСТАНОВКА: (((Материалы * 2) + Цех) * 0.1)
        summary.InstallationServiceCost = (((materialsBase * 2) + summary.WorkshopCost) * 0.1m);

        // ИТОГОВАЯ СТОИМОСТЬ: ((Материалы * 2) + Услуги + Двери) * 1.05
        decimal finalSubTotal = (materialsBase * 2) +
                                (summary.WorkshopCost + summary.PackagingCost + summary.InstallationServiceCost) +
                                summary.TotalDoorCost;

        summary.TotalCost = finalSubTotal * 1.05m;

        return summary;
    }
}