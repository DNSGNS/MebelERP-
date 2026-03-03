//using Android.Widget;
using MyApp1;
using MyApp1.Resources.Other;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
//using static Android.Telephony.CarrierConfigManager;

namespace MyApp1;

public static class ProjectMapper
{
    public static Project MapToApi(ProjectData mauiProject)
    {
        var apiProject = new Project
        {
            Id = mauiProject.Id,
            Status = mauiProject.Status,

            ProjectName = mauiProject.ProjectName,
            CreatorName = App.CurrentUser.Name,
            // PostgreSQL требует UTC
            CreateTime = DateTime.SpecifyKind(mauiProject.CreationDate, DateTimeKind.Utc),

            // Ценовые показатели проекта из ProjectData.cs
            TotalProjectPrice = mauiProject.TotalProjectPrice,
            WorkshopCost = mauiProject.WorkshopCost,
            PackagingCost = mauiProject.PackagingCost,
            InstallationCost = mauiProject.InstallationCost,
            DeliveryCost = mauiProject.DeliveryCost,
            LiftingCost = mauiProject.LiftingCost,
            GarbageRemovalCost = mauiProject.GarbageRemovalCost,
            Prepayment = mauiProject.Prepayment,

            Warehouse = new ProjectWarehouse
            {
                ProjectId = mauiProject.Id,
                IsMaterialReady = mauiProject.IsMaterialReady,
                IsFurnitureReady = mauiProject.IsFurnitureReady
            }
        };

        foreach (var mauiObj in mauiProject.Objects)
        {
            var apiObj = new ProjectObject
            {
                OrderName = mauiObj.ObjectName,
                TotalCost = mauiObj.Summary?.TotalCost ?? 0,
                ImageData = GetImageBytes(mauiObj.ImagePath)
            };

            // 1. ЛДСП (LdspForms -> LdspDetails)
            foreach (var ldsp in mauiObj.LdspForms)
            {
                apiObj.LdspDetails.Add(new LdspDetail
                {
                    Length = ldsp.Length ?? 0.0,
                    Width = ldsp.Width ?? 0.0,
                    Count = ldsp.Count ?? 0,
                    Area = ldsp.Area
                });
            }

            // 2. Фасады (Standard, Special, NonStandard -> FasadDetails)
            MapFasads(mauiObj.StandardFasads, apiObj.FasadDetails);
            MapFasads(mauiObj.SpecialFasads, apiObj.FasadDetails); // Пример маппинга
            MapFasads(mauiObj.NonStandardFasads, apiObj.FasadDetails);

            // 3. Фурнитура
            if (mauiObj.Furniture != null)
            {
                var furnitureDetail = new FurnitureDetail
                {
                    Hdf = mauiObj.Furniture.Hdf ?? 0.0,
                    NaprBez = mauiObj.Furniture.NaprBez ?? 0.0,
                    NaprS = mauiObj.Furniture.NaprS ?? 0.0,
                    PetliBez = mauiObj.Furniture.PetliBez ?? 0.0,
                    PetliS = mauiObj.Furniture.PetliS ?? 0.0,
                    Skoba = mauiObj.Furniture.Skoba ?? 0.0,
                    Nakladnaya = mauiObj.Furniture.Nakladnaya ?? 0.0,
                    Knopka = mauiObj.Furniture.Knopka ?? 0.0,
                    Gola = mauiObj.Furniture.Gola ?? 0.0,
                    Truba = mauiObj.Furniture.Truba ?? 0.0,
                    GazLift = mauiObj.Furniture.GazLift ?? 0.0,
                    Kruchki = mauiObj.Furniture.Kruchki ?? 0.0,
                    Podsvetka = mauiObj.Furniture.Podsvetka ?? 0.0,
                    Comment = mauiObj.Furniture.Comment,

                    // Маппинг коллекции профилей (18 полей)
                    Profiles = mauiObj.Furniture.Profiles.Select(p => new FurnitureProfileDetail
                    {
                        TopGuideName = p.TopGuideName ?? string.Empty,
                        TopGuideSize = p.TopGuideSize,
                        TopGuideCount = p.TopGuideCount,

                        BottomGuideName = p.BottomGuideName ?? string.Empty,
                        BottomGuideSize = p.BottomGuideSize,
                        BottomGuideCount = p.BottomGuideCount,

                        VerticalSlimName = p.VerticalSlimName ?? string.Empty,
                        VerticalSlimSize = p.VerticalSlimSize,
                        VerticalSlimCount = p.VerticalSlimCount,

                        NarrowFrameName = p.NarrowFrameName ?? string.Empty,
                        NarrowFrameSize = p.NarrowFrameSize,
                        NarrowFrameCount = p.NarrowFrameCount,

                        WideFrameName = p.WideFrameName ?? string.Empty,
                        WideFrameSize = p.WideFrameSize,
                        WideFrameCount = p.WideFrameCount,

                        MiddleFrameName = p.MiddleFrameName ?? string.Empty,
                        MiddleFrameSize = p.MiddleFrameSize,
                        MiddleFrameCount = p.MiddleFrameCount
                    }).ToList()
                };

                apiObj.FurnitureDetails.Add(furnitureDetail);
            }

            // 4. Двери (DoorData -> DoorDetails)
            if (mauiObj.DoorData != null)
            {
                var doorDetail = new DoorDetail
                {
                    // Основные параметры проема
                    InstallationTypeIndex = mauiObj.DoorSlimLineData.SelectedInstallationTypeIndex,
                    OpeningHeight = mauiObj.DoorSlimLineData.OpeningHeight,
                    OpeningWidth = mauiObj.DoorSlimLineData.OpeningWidth,
                    ArrangementIndex = mauiObj.DoorSlimLineData.SelectedArrangementIndex,
                    ColorIndex = mauiObj.DoorSlimLineData.SelectedColorIndex,
                    MiddleFramesCountIndex = mauiObj.DoorSlimLineData.MiddleFramesCountIndex,

                    // Маппинг списка вставок (динамический перенос всех строк таблицы)
                    Inserts = mauiObj.DoorSlimLineData.Inserts.Select(i => new SlimInsertDetail
                    {
                        Index = i.Index,
                        Name = i.Name,
                        MaterialIndex = i.MaterialIndex,
                        Height = i.Height,
                        Width = i.Width,
                        Count = i.Count
                    }).ToList()
                };

                apiObj.DoorDetails.Add(doorDetail);
            }


            // 5. Мягкие элементы
            if (mauiObj.SoftlyData != null)
            {
                apiObj.SoftlyDetails.Add(new SoftlyDetail
                {
                    Length = mauiObj.SoftlyData.Length ?? 0.0,
                    Width = mauiObj.SoftlyData.Width ?? 0.0,
                    Count = mauiObj.SoftlyData.Count ?? 0,
                    Area = mauiObj.SoftlyData.Area,
                    Tabletop = mauiObj.SoftlyData.Tabletop ?? 0,
                    Apron = mauiObj.SoftlyData.Apron ?? 0,
                    Additional = mauiObj.SoftlyData.Additional ?? 0,
                    HasBase = mauiObj.SoftlyData.HasBase,
                    HasDryer = mauiObj.SoftlyData.HasDryer,
                    SelectedCategory = (SelectedCategory)mauiObj.SoftlyData.SelectedCategory,
                    ShieldType = (ShieldType)mauiObj.SoftlyData.SelectedShieldType
                });
            }

            // 6. Отчет (Summary -> Report)
            if (mauiObj.Summary != null)
            {
                apiObj.Reports.Add(new Report
                {
                    TotalAreaLdsp = mauiObj.Summary.TotalAreaLdsp,
                    TotalLdspCount = mauiObj.Summary.TotalLdspCount,
                    TotalEdgeCount = mauiObj.Summary.TotalEdgeCount,
                    LdspCost = (decimal)mauiObj.Summary.LdspCost,
                    EdgingCost = (decimal)mauiObj.Summary.EdgingCost,
                    TotalFasadCost = (decimal)mauiObj.Summary.TotalFasadCost,
                    TotalDoorCost = (decimal)mauiObj.Summary.TotalDoorCost,
                    TotalSoftlyCost = (decimal)mauiObj.Summary.TotalSoftlyCost,
                    WorkshopCost = (decimal)mauiObj.Summary.WorkshopCost,
                    PackagingCost = (decimal)mauiObj.Summary.PackagingCost,
                    InstallationServiceCost = (decimal)mauiObj.Summary.InstallationServiceCost,
                    TotalCost = (decimal)mauiObj.Summary.TotalCost,


                });
            }


            //7. Отчёт Раскроя
            if (mauiObj.CuttingLdsp?.SavedReport != null)
                apiObj.CuttingRep.Add(MapCutting(mauiObj.CuttingLdsp.SavedReport, CuttingType.Ldsp));

            if (mauiObj.CuttingFAgt?.SavedReport != null)
                apiObj.CuttingRep.Add(MapCutting(mauiObj.CuttingFAgt.SavedReport, CuttingType.FAgt));

            if (mauiObj.CuttingFLdsp?.SavedReport != null)
                apiObj.CuttingRep.Add(MapCutting(mauiObj.CuttingFLdsp.SavedReport, CuttingType.FLdsp));



            apiProject.ProjectObjects.Add(apiObj);
        }

        return apiProject;
    }

    // Преобразование из API модели в MAUI модель
    public static ProjectData MapToMaui(Project apiProject, bool isInstaller = false)
    {
        DateTime firstInstallDate = DateTime.MinValue;

        var firstInstallDateUtc = apiProject.ProjectWorks?
            .Where(w => w.DidInstallation > 0)
            .Select(w => w.DatePerformed)
            .FirstOrDefault();

        if (firstInstallDateUtc.HasValue)
        {
            firstInstallDate = firstInstallDateUtc.Value.ToLocalTime();
        }
        var mauiProject = new ProjectData
        {
            Id = apiProject.Id,
            Status = apiProject.Status,

            ProjectName = apiProject.ProjectName,
            // Преобразуем UTC из БД в локальное время для мобильного устройства
            CreationDate = isInstaller
            ? firstInstallDate
            : apiProject.CreateTime.ToLocalTime(),

            TotalProjectPrice = apiProject.TotalProjectPrice,
            WorkshopCost = apiProject.WorkshopCost,
            PackagingCost = apiProject.PackagingCost,
            InstallationCost = apiProject.InstallationCost,
            DeliveryCost = apiProject.DeliveryCost,
            LiftingCost = apiProject.LiftingCost,
            GarbageRemovalCost = apiProject.GarbageRemovalCost,
            Prepayment = apiProject.Prepayment,

            IsMaterialReady = apiProject.Warehouse?.IsMaterialReady ?? false,
            IsFurnitureReady = apiProject.Warehouse?.IsFurnitureReady ?? false
        };

        foreach (var apiObj in apiProject.ProjectObjects)
        {
            var mauiObj = new ObjectData
            {
                ObjectName = apiObj.OrderName,
                ProjectName = apiProject.ProjectName,
            };

            // 1. ЛДСП
            foreach (var ldsp in apiObj.LdspDetails)
            {
                mauiObj.LdspForms.Add(new LDSPForm
                {
                    Length = ldsp.Length,
                    Width = ldsp.Width,
                    Count = ldsp.Count,
                    //Area = ldsp.Area
                });
            }

            // 2. Фасады (Распределяем по спискам на основе типа)
            foreach (var fasad in apiObj.FasadDetails)
            {
                var form = new FasadForm
                {
                    Length = fasad.Length,
                    Width = fasad.Width,
                    Count = fasad.Count,
                    //Area = fasad.Area,
                    Color = fasad.Color,
                    MillingText = fasad.Frez,
                    // Обратный маппинг Enums
                    SelectedType = (FasadType)fasad.Type,
                    SelectedEdgeType = fasad.FrezType == FrezType.Freza
                                       ? FasadEdgeType.Freza
                                       : FasadEdgeType.Mylo
                };

                if (fasad.Type == FasadTypeBd.Standard) mauiObj.StandardFasads.Add(form);
                else if (fasad.Type == FasadTypeBd.NonStandard) mauiObj.NonStandardFasads.Add(form);
                else mauiObj.SpecialFasads.Add(form);
            }

            // 3. Фурнитура (в БД List, в MAUI один объект)
            var furn = apiObj.FurnitureDetails.FirstOrDefault();
            if (furn != null)
            {
                mauiObj.Furniture = new FurnitureForm
                {
                    Hdf = furn.Hdf,
                    NaprBez = furn.NaprBez,
                    NaprS = furn.NaprS,
                    PetliBez = furn.PetliBez,
                    PetliS = furn.PetliS,
                    Skoba = furn.Skoba,
                    Nakladnaya = furn.Nakladnaya,
                    Knopka = furn.Knopka,
                    Gola = furn.Gola,
                    Truba = furn.Truba,
                    GazLift = furn.GazLift,
                    Kruchki = furn.Kruchki,
                    Podsvetka = furn.Podsvetka,

                    Comment = furn.Comment
                };

                // Заполнение списка профилей из БД в MAUI форму
                if (furn.Profiles != null && furn.Profiles.Any())
                {
                    mauiObj.Furniture.Profiles.Clear();

                    foreach (var p in furn.Profiles)
                    {
                        mauiObj.Furniture.Profiles.Add(new FurnitureProfileForm
                        {
                            TopGuideName = p.TopGuideName,
                            TopGuideSize = p.TopGuideSize,
                            TopGuideCount = p.TopGuideCount,

                            BottomGuideName = p.BottomGuideName,
                            BottomGuideSize = p.BottomGuideSize,
                            BottomGuideCount = p.BottomGuideCount,

                            VerticalSlimName = p.VerticalSlimName,
                            VerticalSlimSize = p.VerticalSlimSize,
                            VerticalSlimCount = p.VerticalSlimCount,

                            NarrowFrameName = p.NarrowFrameName,
                            NarrowFrameSize = p.NarrowFrameSize,
                            NarrowFrameCount = p.NarrowFrameCount,

                            WideFrameName = p.WideFrameName,
                            WideFrameSize = p.WideFrameSize,
                            WideFrameCount = p.WideFrameCount,

                            MiddleFrameName = p.MiddleFrameName,
                            MiddleFrameSize = p.MiddleFrameSize,
                            MiddleFrameCount = p.MiddleFrameCount
                        });
                    }
                }
            }

            var door = apiObj.DoorDetails.FirstOrDefault();
            if (door != null)
            {
                var newForm = new DoorSlimLineForm
                {
                    OpeningHeight = door.OpeningHeight,
                    OpeningWidth = door.OpeningWidth,
                    SelectedInstallationTypeIndex = door.InstallationTypeIndex,
                    SelectedArrangementIndex = door.ArrangementIndex,
                    SelectedColorIndex = door.ColorIndex,
                    // Эта установка триггерит создание нужного количества строк в Inserts
                    MiddleFramesCountIndex = door.MiddleFramesCountIndex
                };

                // Восстанавливаем данные для каждой конкретной вставки (материал и высоту)
                if (door.Inserts != null)
                {
                    foreach (var dbInsert in door.Inserts)
                    {
                        var formInsert = newForm.Inserts.FirstOrDefault(x => x.Index == dbInsert.Index);
                        if (formInsert != null)
                        {
                            formInsert.MaterialIndex = dbInsert.MaterialIndex;
                            // Первую высоту не трогаем (она расчетная), остальные берем из базы
                            if (dbInsert.Index > 0)
                            {
                                formInsert.Height = dbInsert.Height;
                            }
                        }
                    }
                }

                mauiObj.DoorSlimLineData = newForm;
            }


            // 5. Мягкие элементы
            var soft = apiObj.SoftlyDetails.FirstOrDefault();
            if (soft != null)
            {
                mauiObj.SoftlyData = new SoftlyForm
                {
                    Length = soft.Length,
                    Width = soft.Width,
                    Count = soft.Count,
                    //Area = soft.Area,
                    SelectedCategory = (SoftCategory)soft.SelectedCategory,
                    SelectedShieldType = (SoftShieldType)soft.ShieldType,
                    Tabletop = soft.Tabletop,
                    Apron = soft.Apron,
                    Additional = soft.Additional,
                    HasBase = soft.HasBase,
                    HasDryer = soft.HasDryer
                };
            }

            PriceList prices = new PriceList();
            mauiObj.UpdateCalculation(prices);



            // 7. Раскрой (Cutting)
            // --- 7. РАСКРОЙ (3 ТИПА) ---
            foreach (var apiCutting in apiObj.CuttingRep)
            {
                // Явно приводим apiCutting.Type к int для сравнения в switch, 
                // если apiCutting.Type является Enum-ом в API.
                CuttingData targetMauiCutting = (int)apiCutting.Type switch
                {
                    0 => mauiObj.CuttingLdsp,
                    1 => mauiObj.CuttingFAgt,
                    2 => mauiObj.CuttingFLdsp,
                    _ => null
                };

                if (targetMauiCutting != null)
                {
                    MapCuttingToMaui(apiCutting, targetMauiCutting);
                }
            }

            // 8. Фото объекта
            if (apiObj.ImageData != null && apiObj.ImageData.Length > 0)
            {
                string fileName = $"img_{Guid.NewGuid()}.jpg";
                string localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                File.WriteAllBytes(localPath, apiObj.ImageData);
                mauiObj.ImagePath = localPath;
            }

            mauiProject.Objects.Add(mauiObj);
        }
        mauiProject.RecalculateTotals();
        return mauiProject;
    }

    public static ProjectData MapInstallerToMaui(Project apiProject)
    {

        DateTime firstInstallDate = DateTime.MinValue;

        var firstInstallDateUtc = apiProject.ProjectWorks?
            .Where(w => w.DidInstallation > 0)
            .Select(w => w.DatePerformed)
            .FirstOrDefault();

        if (firstInstallDateUtc.HasValue)
        {
            firstInstallDate = firstInstallDateUtc.Value.ToLocalTime();
        }

        var mauiProject = new ProjectData
        {
            Id = apiProject.Id,
            Status = apiProject.Status,

            ProjectName = apiProject.ProjectName,
            CreationDate = firstInstallDate,

            TotalProjectPrice = apiProject.TotalProjectPrice,
            WorkshopCost = apiProject.WorkshopCost,
            PackagingCost = apiProject.PackagingCost,
            InstallationCost = apiProject.InstallationCost,
            DeliveryCost = apiProject.DeliveryCost,
            LiftingCost = apiProject.LiftingCost,
            GarbageRemovalCost = apiProject.GarbageRemovalCost,
            Prepayment = apiProject.Prepayment,

            IsMaterialReady = apiProject.Warehouse?.IsMaterialReady ?? false,
            IsFurnitureReady = apiProject.Warehouse?.IsFurnitureReady ?? false
        };

        foreach (var apiObj in apiProject.ProjectObjects)
        {
            var mauiObj = new ObjectData
            {
                ObjectName = apiObj.OrderName,
                ProjectName = apiProject.ProjectName,
            };

            // 1. ЛДСП
            foreach (var ldsp in apiObj.LdspDetails)
            {
                mauiObj.LdspForms.Add(new LDSPForm
                {
                    Length = ldsp.Length,
                    Width = ldsp.Width,
                    Count = ldsp.Count,
                    //Area = ldsp.Area
                });
            }

            // 2. Фасады (Распределяем по спискам на основе типа)
            foreach (var fasad in apiObj.FasadDetails)
            {
                var form = new FasadForm
                {
                    Length = fasad.Length,
                    Width = fasad.Width,
                    Count = fasad.Count,
                    //Area = fasad.Area,
                    Color = fasad.Color,
                    MillingText = fasad.Frez,
                    // Обратный маппинг Enums
                    SelectedType = (FasadType)fasad.Type,
                    SelectedEdgeType = fasad.FrezType == FrezType.Freza
                                       ? FasadEdgeType.Freza
                                       : FasadEdgeType.Mylo
                };

                if (fasad.Type == FasadTypeBd.Standard) mauiObj.StandardFasads.Add(form);
                else if (fasad.Type == FasadTypeBd.NonStandard) mauiObj.NonStandardFasads.Add(form);
                else mauiObj.SpecialFasads.Add(form);
            }

            // 3. Фурнитура (в БД List, в MAUI один объект)
            var furn = apiObj.FurnitureDetails.FirstOrDefault();
            if (furn != null)
            {
                mauiObj.Furniture = new FurnitureForm
                {
                    Hdf = furn.Hdf,
                    NaprBez = furn.NaprBez,
                    NaprS = furn.NaprS,
                    PetliBez = furn.PetliBez,
                    PetliS = furn.PetliS,
                    Skoba = furn.Skoba,
                    Nakladnaya = furn.Nakladnaya,
                    Knopka = furn.Knopka,
                    Gola = furn.Gola,
                    Truba = furn.Truba,
                    GazLift = furn.GazLift,
                    Kruchki = furn.Kruchki,
                    Podsvetka = furn.Podsvetka,

                    Comment = furn.Comment
                };

                // Заполнение списка профилей из БД в MAUI форму
                if (furn.Profiles != null && furn.Profiles.Any())
                {
                    mauiObj.Furniture.Profiles.Clear();

                    foreach (var p in furn.Profiles)
                    {
                        mauiObj.Furniture.Profiles.Add(new FurnitureProfileForm
                        {
                            TopGuideName = p.TopGuideName,
                            TopGuideSize = p.TopGuideSize,
                            TopGuideCount = p.TopGuideCount,

                            BottomGuideName = p.BottomGuideName,
                            BottomGuideSize = p.BottomGuideSize,
                            BottomGuideCount = p.BottomGuideCount,

                            VerticalSlimName = p.VerticalSlimName,
                            VerticalSlimSize = p.VerticalSlimSize,
                            VerticalSlimCount = p.VerticalSlimCount,

                            NarrowFrameName = p.NarrowFrameName,
                            NarrowFrameSize = p.NarrowFrameSize,
                            NarrowFrameCount = p.NarrowFrameCount,

                            WideFrameName = p.WideFrameName,
                            WideFrameSize = p.WideFrameSize,
                            WideFrameCount = p.WideFrameCount,

                            MiddleFrameName = p.MiddleFrameName,
                            MiddleFrameSize = p.MiddleFrameSize,
                            MiddleFrameCount = p.MiddleFrameCount
                        });
                    }
                }
            }

            var door = apiObj.DoorDetails.FirstOrDefault();
            if (door != null)
            {
                var newForm = new DoorSlimLineForm
                {
                    OpeningHeight = door.OpeningHeight,
                    OpeningWidth = door.OpeningWidth,
                    SelectedInstallationTypeIndex = door.InstallationTypeIndex,
                    SelectedArrangementIndex = door.ArrangementIndex,
                    SelectedColorIndex = door.ColorIndex,
                    // Эта установка триггерит создание нужного количества строк в Inserts
                    MiddleFramesCountIndex = door.MiddleFramesCountIndex
                };

                // Восстанавливаем данные для каждой конкретной вставки (материал и высоту)
                if (door.Inserts != null)
                {
                    foreach (var dbInsert in door.Inserts)
                    {
                        var formInsert = newForm.Inserts.FirstOrDefault(x => x.Index == dbInsert.Index);
                        if (formInsert != null)
                        {
                            formInsert.MaterialIndex = dbInsert.MaterialIndex;
                            // Первую высоту не трогаем (она расчетная), остальные берем из базы
                            if (dbInsert.Index > 0)
                            {
                                formInsert.Height = dbInsert.Height;
                            }
                        }
                    }
                }

                mauiObj.DoorSlimLineData = newForm;
            }


            // 5. Мягкие элементы
            var soft = apiObj.SoftlyDetails.FirstOrDefault();
            if (soft != null)
            {
                mauiObj.SoftlyData = new SoftlyForm
                {
                    Length = soft.Length,
                    Width = soft.Width,
                    Count = soft.Count,
                    //Area = soft.Area,
                    SelectedCategory = (SoftCategory)soft.SelectedCategory,
                    SelectedShieldType = (SoftShieldType)soft.ShieldType,
                    Tabletop = soft.Tabletop,
                    Apron = soft.Apron,
                    Additional = soft.Additional,
                    HasBase = soft.HasBase,
                    HasDryer = soft.HasDryer
                };
            }

            PriceList prices = new PriceList();
            mauiObj.UpdateCalculation(prices);



            // 7. Раскрой (Cutting)
            // --- 7. РАСКРОЙ (3 ТИПА) ---
            foreach (var apiCutting in apiObj.CuttingRep)
            {
                // Явно приводим apiCutting.Type к int для сравнения в switch, 
                // если apiCutting.Type является Enum-ом в API.
                CuttingData targetMauiCutting = (int)apiCutting.Type switch
                {
                    0 => mauiObj.CuttingLdsp,
                    1 => mauiObj.CuttingFAgt,
                    2 => mauiObj.CuttingFLdsp,
                    _ => null
                };

                if (targetMauiCutting != null)
                {
                    MapCuttingToMaui(apiCutting, targetMauiCutting);
                }
            }

            // 8. Фото объекта
            if (apiObj.ImageData != null && apiObj.ImageData.Length > 0)
            {
                string fileName = $"img_{Guid.NewGuid()}.jpg";
                string localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                File.WriteAllBytes(localPath, apiObj.ImageData);
                mauiObj.ImagePath = localPath;
            }

            mauiProject.Objects.Add(mauiObj);
        }
        mauiProject.RecalculateTotals();
        return mauiProject;
    }


    private static void MapCuttingToMaui(dynamic apiCutting, CuttingData target)
    {
        target.SavedReport = new CuttingSaveForm
        {
            SheetLength = apiCutting.SheetLength,
            SheetWidth = apiCutting.SheetWidth,
            SheetArea = apiCutting.SheetArea,
            MaterialColor = apiCutting.MaterialColor,
            TotalSheets = apiCutting.TotalSheets,
            TotalSheetArea = apiCutting.TotalSheetArea,
            TotalPartsCount = apiCutting.TotalPartsCount,
            TotalPartsArea = apiCutting.TotalPartsArea,
            Edge1Name = apiCutting.Edge1Name,
            Edge1Thickness = apiCutting.Edge1Thickness,
            TotalEdge1 = apiCutting.TotalEdge1,
            Edge2Name = apiCutting.Edge2Name,
            Edge2Thickness = apiCutting.Edge2Thickness,
            TotalEdge2 = apiCutting.TotalEdge2
        };

        // Восстанавливаем чертежи (Layout)
        if (apiCutting.Sheets != null)
        {
            foreach (var sheetDetail in apiCutting.Sheets)
            {
                if (!string.IsNullOrEmpty(sheetDetail.LayoutDataJson))
                {
                    try
                    {
                        // 1. Десериализуем основной чертеж из JSON
                        var sheetLayout = System.Text.Json.JsonSerializer.Deserialize<SheetLayout>(sheetDetail.LayoutDataJson);

                        if (sheetLayout != null)
                        {
                            // 2. ВАЖНО: Восстанавливаем цвет из отдельного поля БД, 
                            // так как при сохранении вы записывали его в ColorName
                            sheetLayout.ColorName = sheetDetail.ColorName;

                            // Если в модели SheetLayout есть номер листа, его тоже можно восстановить
                            // sheetLayout.SheetIndex = sheetDetail.SheetNumber; 

                            target.SavedReport.Sheets.Add(sheetLayout);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка загрузки чертежа листа: {ex.Message}");
                    }
                }
            }
        }

        // Восстанавливаем список деталей
        foreach (var detail in apiCutting.Details)
        {
            target.SavedReport.Details.Add(new CuttingDetails
            {
                Color = detail.Color,
                Length = detail.Length,
                Width = detail.Width,
                Count = detail.Count,
                CanRotate = detail.CanRotate,
                E1L1 = detail.E1L1,
                E1L2 = detail.E1L2,
                E1W1 = detail.E1W1,
                E1W2 = detail.E1W2,
                E2L1 = detail.E2L1,
                E2L2 = detail.E2L2,
                E2W1 = detail.E2W1,
                E2W2 = detail.E2W2
            });
        }
    }

    public static InstallerForm MapToInstallerForm(InstallationInfoResponse response)
    {
        var form = new InstallerForm();

        // 1. Маппинг проектов (используем существующий MapToMaui)
        if (response.Projects != null)
        {
            foreach (var p in response.Projects)
            {
                form.Projects.Add(MapToProjectManage(p));
            }
        }

        // 2. Маппинг всех доступных установщиков
        if (response.AllInstallers != null)
        {
            foreach (var w in response.AllInstallers)
            {
                form.AllAvailableInstallers.Add(MapToWorkMan(w));
            }
        }

        // 3. Маппинг распределенных работников (те, кто уже на замере)
        if (response.AssignedWorkers != null)
        {
            foreach (var w in response.AssignedWorkers)
            {
                form.DistributedInstallers.Add(MapToWorkMan(w));
            }
        }

        return form;
    }





    // Вспомогательный метод для маппинга одного рабочего
    private static ProjectManageData MapToProjectManage(Project apiProj)
    {
        var projectData = new ProjectManageData
        {
            Id = apiProj.Id,
            ProjectName = apiProj.ProjectName,
            CreatorName = apiProj.CreatorName,
            CreationDate = apiProj.CreateTime.ToLocalTime(),
            TotalProjectPrice = apiProj.TotalProjectPrice,
            WorkshopCost = apiProj.WorkshopCost,
            PackagingCost = apiProj.PackagingCost,
            InstallationCost = apiProj.InstallationCost,
            DeliveryCost = apiProj.DeliveryCost,
            LiftingCost = apiProj.LiftingCost,
            GarbageRemovalCost = apiProj.GarbageRemovalCost,
            Prepayment = apiProj.Prepayment,

            // Присваиваем статус (убедитесь, что типы совпадают или приведите)
            Status = (ProjectStatus)apiProj.Status,

            InstallDates = new ObservableCollection<DateTime>(
            apiProj.InstallDates != null
                ? apiProj.InstallDates.Select(d => d.Date.ToLocalTime()).OrderBy(d => d).ToList()
                : new List<DateTime>())
        };

        // НОВОЕ: Заполняем список уже назначенных установщиков
        if (apiProj.ProjectWorks != null)
        {
            foreach (var work in apiProj.ProjectWorks.Where(w => w.DidInstallation > 0))
            {
                // Здесь мы создаем объект WorkMan на основе данных из связи
                // Если в apiProj.ProjectWorks есть навигационное свойство WorkMan, используйте его
                // Если нет, рабочий подтянется по ID (но имя должно быть в ответе API)
                if (work.WorkMan != null)
                {
                    projectData.AssignedInstallers.Add(MapToWorkMan(work.WorkMan));
                }
            }
        }

        return projectData;
    }

    // Вспомогательный метод для маппинга одного рабочего
    private static WorkMan MapToWorkMan(WorkMans apiWorkMan)
    {
        return new WorkMan
        {
            Id = apiWorkMan.Id,
            Name = apiWorkMan.Name,
            // Приведение типов Enum (убедитесь, что индексы совпадают)
            Position = (WorkPosition)apiWorkMan.Position
        };
    }


    // НОВЫЙ МЕТОД: Создание запроса на обновление плана установки
    public static InstallationUpdateDto MapToUpdateManagePage(ProjectManageData project)
    {
        return new InstallationUpdateDto
        {
            ProjectId = project.Id,
            Status = project.Status,

            // Передаем дату
            InstallDates = project.InstallDates
            .Select(d => DateTime.SpecifyKind(d, DateTimeKind.Utc))
            .ToList(),

            InstallerIds = project.AssignedInstallers.Select(w => w.Id).ToList()
        };
    }


    public static Expense MapToApiExpense(Expense mauiExpense)
    {
        // Создаем копию или модифицируем текущий объект для отправки
        var apiExpense = new Expense
        {
            Id = mauiExpense.Id,
            Amount = mauiExpense.Amount,
            Date = DateTime.SpecifyKind(mauiExpense.Date, DateTimeKind.Utc), // Приводим к UTC
            CategoryId = mauiExpense.Category?.Id ?? 0 // Синхронизируем ID из выбранного объекта
        };

        // Важно: Мы НЕ отправляем навигационное свойство Category (объект) целиком,
        // чтобы избежать циклических ссылок и ошибок JSON при десериализации на сервере.
        // Серверу нужен только CategoryId.
        apiExpense.Category = null;

        return apiExpense;
    }



    public static ExpensesForm MapToExpensesForm(ExpensesDataDto dto)
    {
        var form = new ExpensesForm();
        if (dto == null) return form;

        // Заполняем категории
        if (dto.Categories != null)
        {
            foreach (var cat in dto.Categories)
                form.Categories.Add(cat);
        }

        // Заполняем расходы
        if (dto.Expenses != null)
        {
            foreach (var exp in dto.Expenses)
            {
                // Важно: Привязываем объект категории из списка категорий по ID, 
                // чтобы Picker корректно отображал выбранный элемент
                if (exp.CategoryId != 0)
                {
                    exp.Category = form.Categories.FirstOrDefault(c => c.Id == exp.CategoryId);
                }
                form.AllExpenses.Add(exp);
            }
        }

        form.UpdateTotal();
        return form;
    }

    private static void MapFasads(IEnumerable<FasadForm> source, List<FasadDetail> target)
    {
        if (source == null) return;
        foreach (var f in source)
        {
            target.Add(new FasadDetail
            {
                Length = f.Length ?? 0.0,
                Width = f.Width ?? 0.0,
                Count = f.Count ?? 0,
                Area = f.Area,
                Color = f.Color ?? "Не указан",
                Frez = f.MillingText ?? "",

                // Здесь происходит магия выбора типа для каждого отдельного фасада
                Type = f.SelectedType switch
                {
                    FasadType.Standard => FasadTypeBd.Standard,
                    FasadType.AGT => FasadTypeBd.AGT,
                    FasadType.Mirror => FasadTypeBd.Mirror,
                    FasadType.Aluminium => FasadTypeBd.Aluminium,
                    FasadType.LDSP => FasadTypeBd.LDSP,
                    FasadType.NonStandard => FasadTypeBd.NonStandard,
                    _ => FasadTypeBd.Standard
                },

                FrezType = f.SelectedEdgeType == FasadEdgeType.Freza
                           ? FrezType.Freza
                           : FrezType.Mylo
            });
        }
    }

    private static byte[] GetImageBytes(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            return File.ReadAllBytes(path);
        }
        catch { return null; }
    }


    private static CuttingRep MapCutting(CuttingSaveForm savedReport, CuttingType type)
    {
        var cuttingRep = new CuttingRep
        {
            Type = type,
            SheetLength = savedReport.SheetLength,
            SheetWidth = savedReport.SheetWidth,
            SheetArea = savedReport.SheetArea,
            MaterialColor = savedReport.MaterialColor,
            TotalSheets = savedReport.TotalSheets,
            TotalSheetArea = savedReport.TotalSheetArea,
            TotalPartsCount = savedReport.TotalPartsCount,
            TotalPartsArea = savedReport.TotalPartsArea,
            Edge1Name = savedReport.Edge1Name ?? "",
            Edge1Thickness = savedReport.Edge1Thickness,
            TotalEdge1 = savedReport.TotalEdge1,
            Edge2Name = savedReport.Edge2Name ?? "",
            Edge2Thickness = savedReport.Edge2Thickness,
            TotalEdge2 = savedReport.TotalEdge2
        };

        // Маппинг листов
        if (savedReport.Sheets != null)
        {
            int sheetCounter = 1;
            foreach (var sheet in savedReport.Sheets)
            {
                cuttingRep.Sheets.Add(new SheetLayoutDetail
                {
                    SheetNumber = sheetCounter++,
                    ColorName = sheet.ColorName, // Исправлено: теперь есть и в ЛДСП
                    LayoutDataJson = System.Text.Json.JsonSerializer.Serialize(sheet)
                });
            }
        }

        // Маппинг деталей
        if (savedReport.Details != null)
        {
            foreach (var detail in savedReport.Details)
            {
                cuttingRep.Details.Add(new CuttingPartItem
                {
                    Color = detail.Color, // Исправлено: теперь есть и в ЛДСП
                    Length = detail.Length,
                    Width = detail.Width,
                    Count = detail.Count,
                    CanRotate = detail.CanRotate,
                    E1L1 = detail.E1L1,
                    E1L2 = detail.E1L2,
                    E1W1 = detail.E1W1,
                    E1W2 = detail.E1W2,
                    E2L1 = detail.E2L1,
                    E2L2 = detail.E2L2,
                    E2W1 = detail.E2W1,
                    E2W2 = detail.E2W2
                });
            }
        }

        return cuttingRep;
    }

}