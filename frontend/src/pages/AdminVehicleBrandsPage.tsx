import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createVehicleBrand,
  deleteVehicleBrand,
  getVehicleBrands,
  updateVehicleBrand,
  type VehicleBrandDto,
} from "../api/vehicleBrands";
import { extractErrorMessages } from "../api/errors";
import { Button } from "../components/ui/Button";
import { ConfirmButton } from "../components/ui/ConfirmButton";
import { Input } from "../components/ui/Input";
import { Spinner } from "../components/ui/Spinner";

function EditableRow({ brand }: { brand: VehicleBrandDto }) {
  const queryClient = useQueryClient();
  const [editing, setEditing] = useState(false);
  const [name, setName] = useState(brand.name);
  const [error, setError] = useState<string | null>(null);

  const updateMutation = useMutation({
    mutationFn: () => updateVehicleBrand(brand.id, { name }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["vehicle-brands"] });
      setEditing(false);
    },
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteVehicleBrand(brand.id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["vehicle-brands"] }),
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  if (editing) {
    return (
      <li className="flex flex-col gap-2 rounded-lg border border-slate-200 bg-white p-3">
        <div className="flex items-center gap-2">
          <Input value={name} onChange={(e) => setName(e.target.value)} className="flex-1" />
          <Button onClick={() => updateMutation.mutate()} disabled={updateMutation.isPending || !name.trim()}>
            Kaydet
          </Button>
          <Button
            variant="secondary"
            onClick={() => {
              setEditing(false);
              setName(brand.name);
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
      <span className="text-sm text-slate-900">{brand.name}</span>
      <div className="flex items-center gap-2">
        <Button variant="ghost" onClick={() => setEditing(true)}>
          Düzenle
        </Button>
        <ConfirmButton
          label="Sil"
          confirmLabel="Evet, Sil"
          message={`"${brand.name}" markasını silmek istediğinize emin misiniz?`}
          onConfirm={() => deleteMutation.mutate()}
          disabled={deleteMutation.isPending}
        />
      </div>
      {error && <p className="text-sm text-red-600">{error}</p>}
    </li>
  );
}

export function AdminVehicleBrandsPage() {
  const queryClient = useQueryClient();
  const brandsQuery = useQuery({ queryKey: ["vehicle-brands"], queryFn: getVehicleBrands });
  const [newName, setNewName] = useState("");
  const [createError, setCreateError] = useState<string | null>(null);

  const createMutation = useMutation({
    mutationFn: () => createVehicleBrand({ name: newName }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["vehicle-brands"] });
      setNewName("");
    },
    onError: (err) => setCreateError(extractErrorMessages(err)[0]),
  });

  function handleCreate(e: FormEvent) {
    e.preventDefault();
    setCreateError(null);
    createMutation.mutate();
  }

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Araç Markaları</h1>
        <p className="text-sm text-slate-500">Araç markalarını yönetin.</p>
      </div>

      <form onSubmit={handleCreate} className="flex items-end gap-2 rounded-xl border border-slate-200 bg-white shadow-sm p-4">
        <Input
          label="Yeni Marka"
          value={newName}
          onChange={(e) => setNewName(e.target.value)}
          placeholder="Örn. Volkswagen"
          className="flex-1"
          required
        />
        <Button type="submit" disabled={createMutation.isPending || !newName.trim()}>
          Ekle
        </Button>
      </form>
      {createError && <p className="text-sm text-red-600">{createError}</p>}

      {brandsQuery.isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {brandsQuery.isError && (
        <p className="rounded-lg border border-red-100 bg-red-50 p-4 text-sm text-red-700">
          Markalar yüklenirken bir hata oluştu.
        </p>
      )}

      {brandsQuery.data && (
        <ul className="flex flex-col gap-2">
          {brandsQuery.data.map((brand) => (
            <EditableRow key={brand.id} brand={brand} />
          ))}
        </ul>
      )}
    </div>
  );
}
