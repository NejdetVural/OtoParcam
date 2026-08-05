import { useEffect, useRef, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import { getCategories } from "../api/categories";
import { getVehicleBrands } from "../api/vehicleBrands";
import { getVehicleModels } from "../api/vehicleModels";
import {
  addProductCompatibility,
  addProductImage,
  createProduct,
  deleteProductImage,
  getProductById,
  getProductCompatibility,
  hideProduct,
  removeProductCompatibility,
  updateProduct,
  type ProductRequest,
} from "../api/products";
import {
  productColorLabels,
  ProductColor,
  productPositionLabels,
  ProductPosition,
  productSideLabels,
  ProductSide,
} from "../api/types";
import { extractErrorMessages } from "../api/errors";
import { Button } from "../components/ui/Button";
import { ConfirmButton } from "../components/ui/ConfirmButton";
import { Input } from "../components/ui/Input";
import { Spinner } from "../components/ui/Spinner";
import { resolveImageUrl } from "../lib/images";

type FormState = {
  categoryId: string;
  vehicleBrandId: string;
  sourceVehicleModelId: string;
  price: string;
  color: ProductColor;
  side: string;
  position: string;
  description: string;
};

const emptyForm: FormState = {
  categoryId: "",
  vehicleBrandId: "",
  sourceVehicleModelId: "",
  price: "",
  color: ProductColor.Other,
  side: "",
  position: "",
  description: "",
};

function toRequest(form: FormState): ProductRequest {
  return {
    categoryId: form.categoryId,
    sourceVehicleModelId: form.sourceVehicleModelId,
    price: form.price.trim() ? Number(form.price) : null,
    color: form.color,
    side: form.side ? (Number(form.side) as ProductSide) : null,
    position: form.position ? (Number(form.position) as ProductPosition) : null,
    description: form.description.trim() ? form.description.trim() : null,
  };
}

function ImagesSection({ productId }: { productId: string }) {
  const queryClient = useQueryClient();
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const productQuery = useQuery({ queryKey: ["product", productId], queryFn: () => getProductById(productId) });

  const addMutation = useMutation({
    mutationFn: () => addProductImage(productId, selectedFile!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["product", productId] });
      setSelectedFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
      setError(null);
    },
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  const deleteMutation = useMutation({
    mutationFn: (imageId: string) => deleteProductImage(productId, imageId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["product", productId] }),
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  const images = [...(productQuery.data?.images ?? [])].sort((a, b) => a.displayOrder - b.displayOrder);

  return (
    <div className="flex flex-col gap-3 rounded-xl border border-slate-200 bg-white shadow-sm p-5">
      <h2 className="text-sm font-medium text-slate-900">Görseller</h2>
      <p className="text-xs text-slate-500">
        Ürün en fazla 10 görsele sahip olabilir. JPEG, PNG veya WebP, en fazla 5 MB.
      </p>

      {images.length > 0 && (
        <div className="flex flex-wrap gap-3">
          {images.map((image) => (
            <div key={image.id} className="relative h-20 w-20 overflow-hidden rounded-lg border border-slate-200">
              <img src={resolveImageUrl(image.imageUrl)} alt="" className="h-full w-full object-cover" />
              <button
                type="button"
                onClick={() => deleteMutation.mutate(image.id)}
                disabled={deleteMutation.isPending}
                className="absolute right-1 top-1 flex h-5 w-5 items-center justify-center rounded-full bg-white/90 text-xs text-slate-700 hover:bg-white"
              >
                ×
              </button>
            </div>
          ))}
        </div>
      )}

      <div className="flex items-end gap-2">
        <label className="flex flex-1 flex-col gap-1.5 text-sm">
          <span className="font-medium text-slate-700">Görsel Dosyası</span>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/jpeg,image/png,image/webp"
            onChange={(e) => setSelectedFile(e.target.files?.[0] ?? null)}
            className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm file:mr-3 file:rounded-md file:border-0 file:bg-slate-100 file:px-2.5 file:py-1 file:text-sm file:font-medium file:text-slate-700 hover:file:bg-slate-200"
          />
        </label>
        <Button type="button" onClick={() => addMutation.mutate()} disabled={addMutation.isPending || !selectedFile}>
          {addMutation.isPending ? "Yükleniyor…" : "Ekle"}
        </Button>
      </div>
      {error && <p className="text-sm text-red-600">{error}</p>}
    </div>
  );
}

function CompatibilitySection({ productId, sourceVehicleModelId }: { productId: string; sourceVehicleModelId: string }) {
  const queryClient = useQueryClient();
  const [selectedModelId, setSelectedModelId] = useState("");
  const [error, setError] = useState<string | null>(null);

  const compatibilityQuery = useQuery({
    queryKey: ["product-compatibility", productId],
    queryFn: () => getProductCompatibility(productId),
  });
  const vehicleModelsQuery = useQuery({ queryKey: ["vehicle-models"], queryFn: getVehicleModels });

  const addMutation = useMutation({
    mutationFn: () => addProductCompatibility(productId, selectedModelId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["product-compatibility", productId] });
      setSelectedModelId("");
      setError(null);
    },
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  const removeMutation = useMutation({
    mutationFn: (vehicleModelId: string) => removeProductCompatibility(productId, vehicleModelId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["product-compatibility", productId] }),
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  const compatibleIds = new Set(compatibilityQuery.data?.map((c) => c.vehicleModelId) ?? []);
  const availableModels = (vehicleModelsQuery.data ?? []).filter(
    (m) => m.id !== sourceVehicleModelId && !compatibleIds.has(m.id),
  );

  return (
    <div className="flex flex-col gap-3 rounded-xl border border-slate-200 bg-white shadow-sm p-5">
      <h2 className="text-sm font-medium text-slate-900">Uyumlu Araçlar</h2>
      <p className="text-xs text-slate-500">
        Kaynak araç dışında bu parçanın uyumlu olduğu diğer modelleri belirtin.
      </p>

      {compatibilityQuery.data && compatibilityQuery.data.length > 0 && (
        <ul className="flex flex-col gap-2">
          {compatibilityQuery.data.map((vehicle) => (
            <li
              key={vehicle.vehicleModelId}
              className="flex items-center justify-between gap-2 rounded-lg border border-slate-100 bg-slate-50 p-2 text-sm"
            >
              <span>
                {vehicle.vehicleBrandName} {vehicle.vehicleModelName}
                {vehicle.variant ? ` (${vehicle.variant})` : ""} — {vehicle.startYear}-{vehicle.endYear}
              </span>
              <Button
                variant="ghost"
                disabled={removeMutation.isPending}
                onClick={() => removeMutation.mutate(vehicle.vehicleModelId)}
              >
                Kaldır
              </Button>
            </li>
          ))}
        </ul>
      )}

      <div className="flex items-end gap-2">
        <select
          value={selectedModelId}
          onChange={(e) => setSelectedModelId(e.target.value)}
          className="flex-1 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
        >
          <option value="">Araç modeli seçin</option>
          {availableModels.map((model) => (
            <option key={model.id} value={model.id}>
              {model.name}
              {model.variant ? ` (${model.variant})` : ""} — {model.startYear}-{model.endYear}
            </option>
          ))}
        </select>
        <Button type="button" onClick={() => addMutation.mutate()} disabled={addMutation.isPending || !selectedModelId}>
          Ekle
        </Button>
      </div>
      {error && <p className="text-sm text-red-600">{error}</p>}
    </div>
  );
}

export function AdminProductFormPage() {
  const { id } = useParams<{ id: string }>();
  const isEdit = Boolean(id);
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [form, setForm] = useState<FormState>(emptyForm);
  const [saveError, setSaveError] = useState<string[]>([]);
  const [savedAt, setSavedAt] = useState<number | null>(null);

  const categoriesQuery = useQuery({ queryKey: ["categories"], queryFn: getCategories });
  const vehicleBrandsQuery = useQuery({ queryKey: ["vehicle-brands"], queryFn: getVehicleBrands });
  const vehicleModelsQuery = useQuery({ queryKey: ["vehicle-models"], queryFn: getVehicleModels });
  const productQuery = useQuery({
    queryKey: ["product", id],
    queryFn: () => getProductById(id!),
    enabled: isEdit,
  });

  useEffect(() => {
    if (productQuery.data) {
      setForm({
        categoryId: productQuery.data.categoryId,
        vehicleBrandId: productQuery.data.vehicleBrandId,
        sourceVehicleModelId: productQuery.data.sourceVehicleModelId,
        price: productQuery.data.price !== null ? String(productQuery.data.price) : "",
        color: productQuery.data.color,
        side: productQuery.data.side !== null ? String(productQuery.data.side) : "",
        position: productQuery.data.position !== null ? String(productQuery.data.position) : "",
        description: productQuery.data.description ?? "",
      });
    }
  }, [productQuery.data]);

  const createMutation = useMutation({
    mutationFn: () => createProduct(toRequest(form)),
    onSuccess: (product) => {
      queryClient.invalidateQueries({ queryKey: ["admin-products"] });
      navigate(`/admin/urunler/${product.id}/duzenle`);
    },
    onError: (err) => setSaveError(extractErrorMessages(err)),
  });

  const updateMutation = useMutation({
    mutationFn: () => updateProduct(id!, toRequest(form)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["product", id] });
      queryClient.invalidateQueries({ queryKey: ["admin-products"] });
      setSaveError([]);
      setSavedAt(Date.now());
    },
    onError: (err) => setSaveError(extractErrorMessages(err)),
  });

  const hideMutation = useMutation({
    mutationFn: () => hideProduct(id!),
    onSuccess: () => navigate("/admin/urunler"),
    onError: () => setSaveError(["Ürün gizlenirken bir hata oluştu."]),
  });

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setSaveError([]);
    setSavedAt(null);
    if (isEdit) {
      updateMutation.mutate();
    } else {
      createMutation.mutate();
    }
  }

  if (isEdit && productQuery.isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    );
  }

  if (isEdit && (productQuery.isError || !productQuery.data)) {
    return <p className="rounded-lg border border-red-100 bg-red-50 p-4 text-sm text-red-700">Ürün bulunamadı.</p>;
  }

  const modelsForBrand = (vehicleModelsQuery.data ?? []).filter((m) => m.vehicleBrandId === form.vehicleBrandId);
  const isSaving = createMutation.isPending || updateMutation.isPending;

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">{isEdit ? "Ürünü Düzenle" : "Yeni Ürün"}</h1>
          {isEdit && productQuery.data && <p className="text-sm text-slate-500">{productQuery.data.title}</p>}
        </div>
        {isEdit && (
          <ConfirmButton
            label="Ürünü Gizle"
            confirmLabel="Evet, Gizle"
            message="Bu ürünü gizlemek istediğinize emin misiniz? Müşteriler artık göremeyecek."
            triggerVariant="secondary"
            disabled={hideMutation.isPending}
            onConfirm={() => hideMutation.mutate()}
          />
        )}
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4 rounded-xl border border-slate-200 bg-white shadow-sm p-6">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium text-slate-700">Kategori</span>
            <select
              value={form.categoryId}
              onChange={(e) => setForm((p) => ({ ...p, categoryId: e.target.value }))}
              required
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
            >
              <option value="" disabled>
                Kategori seçin
              </option>
              {categoriesQuery.data?.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium text-slate-700">Renk</span>
            <select
              value={form.color}
              onChange={(e) => setForm((p) => ({ ...p, color: Number(e.target.value) as ProductColor }))}
              required
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
            >
              {Object.entries(productColorLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium text-slate-700">Kaynak Araç Markası</span>
            <select
              value={form.vehicleBrandId}
              onChange={(e) => setForm((p) => ({ ...p, vehicleBrandId: e.target.value, sourceVehicleModelId: "" }))}
              required
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
            >
              <option value="" disabled>
                Marka seçin
              </option>
              {vehicleBrandsQuery.data?.map((brand) => (
                <option key={brand.id} value={brand.id}>
                  {brand.name}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium text-slate-700">Kaynak Araç Modeli</span>
            <select
              value={form.sourceVehicleModelId}
              onChange={(e) => setForm((p) => ({ ...p, sourceVehicleModelId: e.target.value }))}
              required
              disabled={!form.vehicleBrandId}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400 disabled:bg-slate-50"
            >
              <option value="" disabled>
                Model seçin
              </option>
              {modelsForBrand.map((model) => (
                <option key={model.id} value={model.id}>
                  {model.name}
                  {model.variant ? ` (${model.variant})` : ""} — {model.startYear}-{model.endYear}
                </option>
              ))}
            </select>
          </label>

          <Input
            label="Fiyat (opsiyonel)"
            type="number"
            min={0}
            step="0.01"
            value={form.price}
            onChange={(e) => setForm((p) => ({ ...p, price: e.target.value }))}
            placeholder="Boş bırakılırsa telefonla pazarlık"
          />

          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium text-slate-700">Taraf (opsiyonel)</span>
            <select
              value={form.side}
              onChange={(e) => setForm((p) => ({ ...p, side: e.target.value }))}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
            >
              <option value="">Yok</option>
              {Object.entries(productSideLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </label>

          <label className="flex flex-col gap-1.5 text-sm">
            <span className="font-medium text-slate-700">Konum (opsiyonel)</span>
            <select
              value={form.position}
              onChange={(e) => setForm((p) => ({ ...p, position: e.target.value }))}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
            >
              <option value="">Yok</option>
              {Object.entries(productPositionLabels).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </select>
          </label>
        </div>

        <label className="flex flex-col gap-1.5 text-sm">
          <span className="font-medium text-slate-700">Açıklama (opsiyonel)</span>
          <textarea
            value={form.description}
            onChange={(e) => setForm((p) => ({ ...p, description: e.target.value }))}
            rows={4}
            maxLength={2000}
            className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
          />
        </label>

        {saveError.length > 0 && (
          <ul className="flex flex-col gap-1 rounded-lg border border-red-100 bg-red-50 p-3 text-sm text-red-700">
            {saveError.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        )}

        {savedAt && !isSaving && (
          <p className="rounded-lg border border-emerald-100 bg-emerald-50 p-3 text-sm text-emerald-700">Kaydedildi.</p>
        )}

        <Button type="submit" disabled={isSaving} className="self-start">
          {isSaving ? "Kaydediliyor…" : isEdit ? "Kaydet" : "Ürünü Oluştur"}
        </Button>
      </form>

      {isEdit && id && (
        <>
          <ImagesSection productId={id} />
          <CompatibilitySection productId={id} sourceVehicleModelId={form.sourceVehicleModelId} />
        </>
      )}
    </div>
  );
}
