import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createVehicleModel,
  deleteVehicleModel,
  getVehicleModels,
  updateVehicleModel,
  type VehicleModelDto,
  type VehicleModelRequest,
} from "../api/vehicleModels";
import { getVehicleBrands, type VehicleBrandDto } from "../api/vehicleBrands";
import { extractErrorMessages } from "../api/errors";
import { Button } from "../components/ui/Button";
import { ConfirmButton } from "../components/ui/ConfirmButton";
import { Input } from "../components/ui/Input";
import { Spinner } from "../components/ui/Spinner";

type FormState = {
  vehicleBrandId: string;
  name: string;
  startYear: string;
  endYear: string;
  variant: string;
};

function toRequest(form: FormState): VehicleModelRequest {
  return {
    vehicleBrandId: form.vehicleBrandId,
    name: form.name,
    startYear: Number(form.startYear),
    endYear: Number(form.endYear),
    variant: form.variant.trim() ? form.variant.trim() : null,
  };
}

function BrandSelect({
  value,
  onChange,
  brands,
}: {
  value: string;
  onChange: (value: string) => void;
  brands: VehicleBrandDto[];
}) {
  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value)}
      required
      className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
    >
      <option value="" disabled>
        Marka seçin
      </option>
      {brands.map((brand) => (
        <option key={brand.id} value={brand.id}>
          {brand.name}
        </option>
      ))}
    </select>
  );
}

function EditableRow({ model, brands }: { model: VehicleModelDto; brands: VehicleBrandDto[] }) {
  const queryClient = useQueryClient();
  const [editing, setEditing] = useState(false);
  const [form, setForm] = useState<FormState>({
    vehicleBrandId: model.vehicleBrandId,
    name: model.name,
    startYear: String(model.startYear),
    endYear: String(model.endYear),
    variant: model.variant ?? "",
  });
  const [error, setError] = useState<string | null>(null);
  const brandName = brands.find((b) => b.id === model.vehicleBrandId)?.name ?? "—";

  const updateMutation = useMutation({
    mutationFn: () => updateVehicleModel(model.id, toRequest(form)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["vehicle-models"] });
      setEditing(false);
    },
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteVehicleModel(model.id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["vehicle-models"] }),
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  if (editing) {
    return (
      <li className="flex flex-col gap-2 rounded-lg border border-slate-200 bg-white p-3">
        <div className="flex flex-wrap items-end gap-2">
          <BrandSelect
            value={form.vehicleBrandId}
            onChange={(v) => setForm((p) => ({ ...p, vehicleBrandId: v }))}
            brands={brands}
          />
          <Input
            value={form.name}
            onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
            placeholder="Model adı"
            required
          />
          <Input
            type="number"
            value={form.startYear}
            onChange={(e) => setForm((p) => ({ ...p, startYear: e.target.value }))}
            placeholder="Başlangıç yılı"
            className="w-32"
            required
          />
          <Input
            type="number"
            value={form.endYear}
            onChange={(e) => setForm((p) => ({ ...p, endYear: e.target.value }))}
            placeholder="Bitiş yılı"
            className="w-32"
            required
          />
          <Input
            value={form.variant}
            onChange={(e) => setForm((p) => ({ ...p, variant: e.target.value }))}
            placeholder="Varyant (opsiyonel)"
          />
          <Button onClick={() => updateMutation.mutate()} disabled={updateMutation.isPending}>
            Kaydet
          </Button>
          <Button
            variant="secondary"
            onClick={() => {
              setEditing(false);
              setError(null);
            }}
          >
            Vazgeç
          </Button>
        </div>
        {error && <p className="text-sm text-red-600">{error}</p>}
      </li>
    );
  }

  return (
    <li className="flex items-center justify-between gap-2 rounded-lg border border-slate-200 bg-white p-3">
      <span className="text-sm text-slate-900">
        {brandName} {model.name}
        {model.variant ? ` (${model.variant})` : ""} — {model.startYear}-{model.endYear}
      </span>
      <div className="flex items-center gap-2">
        <Button variant="ghost" onClick={() => setEditing(true)}>
          Düzenle
        </Button>
        <ConfirmButton
          label="Sil"
          confirmLabel="Evet, Sil"
          message={`"${brandName} ${model.name}" modelini silmek istediğinize emin misiniz?`}
          onConfirm={() => deleteMutation.mutate()}
          disabled={deleteMutation.isPending}
        />
      </div>
      {error && <p className="text-sm text-red-600">{error}</p>}
    </li>
  );
}

const emptyForm: FormState = { vehicleBrandId: "", name: "", startYear: "", endYear: "", variant: "" };

export function AdminVehicleModelsPage() {
  const queryClient = useQueryClient();
  const modelsQuery = useQuery({ queryKey: ["vehicle-models"], queryFn: getVehicleModels });
  const brandsQuery = useQuery({ queryKey: ["vehicle-brands"], queryFn: getVehicleBrands });
  const [form, setForm] = useState<FormState>(emptyForm);
  const [createError, setCreateError] = useState<string | null>(null);

  const createMutation = useMutation({
    mutationFn: () => createVehicleModel(toRequest(form)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["vehicle-models"] });
      setForm(emptyForm);
    },
    onError: (err) => setCreateError(extractErrorMessages(err)[0]),
  });

  function handleCreate(e: FormEvent) {
    e.preventDefault();
    setCreateError(null);
    createMutation.mutate();
  }

  const brands = brandsQuery.data ?? [];

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Araç Modelleri</h1>
        <p className="text-sm text-slate-500">Araç modellerini yönetin. Her modelin bir markası olmalıdır.</p>
      </div>

      <form onSubmit={handleCreate} className="flex flex-wrap items-end gap-2 rounded-xl border border-slate-200 bg-white shadow-sm p-4">
        <label className="flex flex-col gap-1.5 text-sm">
          <span className="font-medium text-slate-700">Marka</span>
          <BrandSelect value={form.vehicleBrandId} onChange={(v) => setForm((p) => ({ ...p, vehicleBrandId: v }))} brands={brands} />
        </label>
        <Input
          label="Model Adı"
          value={form.name}
          onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
          placeholder="Örn. Golf"
          required
        />
        <Input
          label="Başlangıç Yılı"
          type="number"
          value={form.startYear}
          onChange={(e) => setForm((p) => ({ ...p, startYear: e.target.value }))}
          className="w-32"
          required
        />
        <Input
          label="Bitiş Yılı"
          type="number"
          value={form.endYear}
          onChange={(e) => setForm((p) => ({ ...p, endYear: e.target.value }))}
          className="w-32"
          required
        />
        <Input
          label="Varyant (opsiyonel)"
          value={form.variant}
          onChange={(e) => setForm((p) => ({ ...p, variant: e.target.value }))}
          placeholder="Örn. GTI"
        />
        <Button type="submit" disabled={createMutation.isPending || !form.vehicleBrandId || !form.name.trim()}>
          Ekle
        </Button>
      </form>
      {createError && <p className="text-sm text-red-600">{createError}</p>}
      {brands.length === 0 && !brandsQuery.isLoading && (
        <p className="text-sm text-amber-700">Önce en az bir araç markası eklemeniz gerekiyor.</p>
      )}

      {modelsQuery.isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {modelsQuery.isError && (
        <p className="rounded-lg border border-red-100 bg-red-50 p-4 text-sm text-red-700">
          Modeller yüklenirken bir hata oluştu.
        </p>
      )}

      {modelsQuery.data && (
        <ul className="flex flex-col gap-2">
          {modelsQuery.data.map((model) => (
            <EditableRow key={model.id} model={model} brands={brands} />
          ))}
        </ul>
      )}
    </div>
  );
}
