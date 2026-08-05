import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createCategory, deleteCategory, getCategories, updateCategory, type CategoryDto } from "../api/categories";
import { extractErrorMessages } from "../api/errors";
import { Button } from "../components/ui/Button";
import { ConfirmButton } from "../components/ui/ConfirmButton";
import { Input } from "../components/ui/Input";
import { Spinner } from "../components/ui/Spinner";

function EditableRow({ category }: { category: CategoryDto }) {
  const queryClient = useQueryClient();
  const [editing, setEditing] = useState(false);
  const [name, setName] = useState(category.name);
  const [error, setError] = useState<string | null>(null);

  const updateMutation = useMutation({
    mutationFn: () => updateCategory(category.id, { name }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      setEditing(false);
    },
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteCategory(category.id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["categories"] }),
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
              setName(category.name);
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
      <span className="text-sm text-slate-900">{category.name}</span>
      <div className="flex items-center gap-2">
        <Button variant="ghost" onClick={() => setEditing(true)}>
          Düzenle
        </Button>
        <ConfirmButton
          label="Sil"
          confirmLabel="Evet, Sil"
          message={`"${category.name}" kategorisini silmek istediğinize emin misiniz?`}
          onConfirm={() => deleteMutation.mutate()}
          disabled={deleteMutation.isPending}
        />
      </div>
      {error && <p className="text-sm text-red-600">{error}</p>}
    </li>
  );
}

export function AdminCategoriesPage() {
  const queryClient = useQueryClient();
  const categoriesQuery = useQuery({ queryKey: ["categories"], queryFn: getCategories });
  const [newName, setNewName] = useState("");
  const [createError, setCreateError] = useState<string | null>(null);

  const createMutation = useMutation({
    mutationFn: () => createCategory({ name: newName }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
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
        <h1 className="text-2xl font-semibold text-slate-900">Kategoriler</h1>
        <p className="text-sm text-slate-500">Ürün kategorilerini yönetin.</p>
      </div>

      <form onSubmit={handleCreate} className="flex items-end gap-2 rounded-xl border border-slate-200 bg-white shadow-sm p-4">
        <Input
          label="Yeni Kategori"
          value={newName}
          onChange={(e) => setNewName(e.target.value)}
          placeholder="Örn. Fren Sistemi"
          className="flex-1"
          required
        />
        <Button type="submit" disabled={createMutation.isPending || !newName.trim()}>
          Ekle
        </Button>
      </form>
      {createError && <p className="text-sm text-red-600">{createError}</p>}

      {categoriesQuery.isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {categoriesQuery.isError && (
        <p className="rounded-lg border border-red-100 bg-red-50 p-4 text-sm text-red-700">
          Kategoriler yüklenirken bir hata oluştu.
        </p>
      )}

      {categoriesQuery.data && (
        <ul className="flex flex-col gap-2">
          {categoriesQuery.data.map((category) => (
            <EditableRow key={category.id} category={category} />
          ))}
        </ul>
      )}
    </div>
  );
}
