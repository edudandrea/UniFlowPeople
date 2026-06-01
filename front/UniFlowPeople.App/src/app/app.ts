import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { AdmissaoPage } from './pages/admissao-page/admissao-page';
import { BeneficiosPage } from './pages/beneficios-page/beneficios-page';
import { CargosPage } from './pages/cargos-page/cargos-page';
import { ColaboradoresPage } from './pages/colaboradores-page/colaboradores-page';
import { ContratosPage } from './pages/contratos-page/contratos-page';
import { DashboardPage } from './pages/dashboard-page/dashboard-page';
import { DemissaoPage } from './pages/demissao-page/demissao-page';
import { DepartamentosPage } from './pages/departamentos-page/departamentos-page';
import { DocumentosPage } from './pages/documentos-page/documentos-page';
import { EmpresasPage } from './pages/empresas-page/empresas-page';
import { EstruturaPage } from './pages/estrutura-page/estrutura-page';
import { FeriasPage } from './pages/ferias-page/ferias-page';
import { PontoPage } from './pages/ponto-page/ponto-page';
import { RecrutamentoPage } from './pages/recrutamento-page/recrutamento-page';
import { TreinamentosPage } from './pages/treinamentos-page/treinamentos-page';
import { UsuariosPage } from './pages/usuarios-page/usuarios-page';

type Role = 'SistemaAdmin' | 'EmpresaAdmin' | 'RH' | 'Gestor' | 'Colaborador';
type MessageType = 'success' | 'error' | 'info';

interface UsuarioInfo {
  id: number;
  empresaId?: number | null;
  colaboradorId?: number | null;
  nome: string;
  login: string;
  email: string;
  role: Role;
}

interface AuthResponse {
  token: string;
  expiresAt: string;
  usuario: UsuarioInfo;
}

interface Empresa {
  id?: number;
  razaoSocial: string;
  nomeFantasia: string;
  cnpj: string;
  telefone?: string;
  email?: string;
  endereco?: string;
  cidade?: string;
  estado?: string;
  cep?: string;
  ativo: boolean;
}

interface Contrato {
  id?: number;
  empresaId: number;
  plano: string;
  status: string;
  dataInicio: string;
  dataFim?: string;
  limiteColaboradores: number;
  valorMensal: number;
  observacoes?: string;
  empresa?: Empresa;
}

interface UsuarioCreate {
  empresaId?: number;
  colaboradorId?: number;
  nome: string;
  login: string;
  email: string;
  senha: string;
  role: Role;
  ativo: boolean;
}

interface ModuleItem {
  id: string;
  label: string;
  icon: string;
  description: string;
}

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    FormsModule,
    AdmissaoPage,
    BeneficiosPage,
    CargosPage,
    ColaboradoresPage,
    ContratosPage,
    DashboardPage,
    DemissaoPage,
    DepartamentosPage,
    DocumentosPage,
    EmpresasPage,
    EstruturaPage,
    FeriasPage,
    PontoPage,
    RecrutamentoPage,
    TreinamentosPage,
    UsuariosPage,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  vm = this;
  private readonly http = inject(HttpClient);
  private readonly api = 'http://localhost:5036/api';

  token = signal(localStorage.getItem('uniflow.token') ?? '');
  usuario = signal<UsuarioInfo | null>(
    JSON.parse(localStorage.getItem('uniflow.usuario') ?? 'null') as UsuarioInfo | null,
  );

  activeModule = signal('dashboard');
  loading = signal(false);
  mensagem = signal('');
  mensagemTipo = signal<MessageType>('info');
  userMenuOpen = signal(false);
  cadastroMenuOpen = signal(true);
  peopleMenuOpen = signal(true);
  profileModalOpen = signal(false);
  passwordModalOpen = signal(false);
  admissaoColaboradorModalOpen = signal(false);
  colaboradorModalOpen = signal(false);
  confirmDelete = signal<{ endpoint: string; id: number; label: string } | null>(null);
  search = signal('');
  curriculoSearch = signal('');
  colaboradorSearch = signal('');
  colaboradoresTab = signal<'pesquisa' | 'dados'>('pesquisa');
  empresaLogada = signal<Empresa | null>(JSON.parse(localStorage.getItem('uniflow.empresa') ?? 'null') as Empresa | null);
  profilePhotoUrl = signal(localStorage.getItem('uniflow.profilePhoto') ?? '');

  loginForm = { login: 'admin', senha: 'Admin@123' };
  bootstrapForm = {
    nome: 'Administrador do Sistema',
    login: 'admin',
    email: 'admin@uniflowpeople.com',
    senha: 'Admin@123',
  };

  profileForm = { nome: '', email: '' };
  passwordForm = { senhaAtual: '', novaSenha: '', confirmarSenha: '' };

  empresaForm: Empresa = this.novaEmpresa();
  contratoForm: Contrato = this.novoContrato();
  usuarioForm: UsuarioCreate = this.novoUsuario();
  filialForm: any = this.novaFilial();
  departamentoForm: any = this.novoDepartamento();
  cargoForm: any = this.novoCargo();
  colaboradorForm: any = this.novoColaborador();
  beneficioForm: any = this.novoBeneficio();
  vagaForm: any = this.novaVaga();
  candidatoForm: any = this.novoCandidato();
  curriculoForm: any = this.novoCurriculo();
  curriculoArquivo: File | null = null;
  treinamentoForm: any = this.novoTreinamento();
  treinamentoColaboradorForm: any = this.novoTreinamentoColaborador();
  admissaoForm: any = this.novaAdmissao();
  demissaoForm: any = this.novaDemissao();
  feriasForm: any = this.novaFerias();
  pontoForm: any = this.novoPonto();
  documentoForm: any = this.novoDocumento();
  beneficioColaboradorForm: any = this.novoBeneficioColaborador();

  empresas = signal<Empresa[]>([]);
  contratos = signal<Contrato[]>([]);
  usuarios = signal<UsuarioInfo[]>([]);
  filiais = signal<any[]>([]);
  departamentos = signal<any[]>([]);
  cargos = signal<any[]>([]);
  colaboradores = signal<any[]>([]);
  beneficios = signal<any[]>([]);
  vagas = signal<any[]>([]);
  candidatos = signal<any[]>([]);
  curriculos = signal<any[]>([]);
  treinamentos = signal<any[]>([]);
  treinamentosColaboradores = signal<any[]>([]);
  admissoes = signal<any[]>([]);
  demissoes = signal<any[]>([]);
  documentosInstitucionais = signal<any[]>([]);
  ferias = signal<any[]>([]);
  pontos = signal<any[]>([]);
  documentos = signal<any[]>([]);
  beneficiosColaboradores = signal<any[]>([]);

  metricas = signal({
    empresasContratantes: 0,
    pagamentosEfetuados: 0,
    pagamentosPendentes: 0,
    contratosAVencer: 0,
    contratosVencidos: 0,
  });

  isLogged = computed(() => !!this.token());
  isSistemaAdmin = computed(() => this.usuario()?.role === 'SistemaAdmin');
  userName = computed(() => this.usuario()?.nome ?? '');
  topbarEyebrow = computed(() => {
    if (this.isSistemaAdmin()) return 'Backoffice SaaS';
    const empresa = this.empresaLogada();
    return empresa?.nomeFantasia || empresa?.razaoSocial || this.usuario()?.role || 'Portal da Empresa';
  });
  userInitials = computed(() =>
    (this.usuario()?.nome ?? 'UF')
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((x) => x[0])
      .join('')
      .toUpperCase(),
  );

  pageTitle = computed(
    () =>
      this.modules().find((x) => x.id === this.activeModule())?.label ??
      this.cadastroModules().find((x) => x.id === this.activeModule())?.label ??
      this.peopleModules().find((x) => x.id === this.activeModule())?.label ??
      'Dashboard',
  );
  pageDescription = computed(
    () =>
      this.modules().find((x) => x.id === this.activeModule())?.description ??
      this.cadastroModules().find((x) => x.id === this.activeModule())?.description ??
      this.peopleModules().find((x) => x.id === this.activeModule())?.description ??
      'Gestão inteligente de pessoas.',
  );

  modules = computed<ModuleItem[]>(() =>
    this.isSistemaAdmin()
      ? [
          { id: 'contratos', label: 'Contratos', icon: '◷', description: 'Planos, vencimentos e situação financeira.' },
          { id: 'dashboard', label: 'Dashboard SaaS', icon: '◈', description: 'Contratos, pagamentos e saúde da carteira.' },
          { id: 'empresas', label: 'Empresas', icon: '▣', description: 'Cadastro e manutenção das empresas contratantes.' },
          { id: 'usuarios', label: 'Usuários', icon: '◎', description: 'Acessos administrativos das empresas.' },
        ]
      : [
          { id: 'dashboard', label: 'Visão geral', icon: '✦', description: 'Indicadores rápidos do RH.' },
          { id: 'beneficios', label: 'Benefícios', icon: '◆', description: 'Benefícios e vínculos com colaboradores.' },
          { id: 'documentos', label: 'Documentos', icon: '▧', description: 'Documentos e validações dos colaboradores.' },
          { id: 'estrutura', label: 'Filiais', icon: '▤', description: 'Unidades e filiais da empresa.' },
          { id: 'ferias', label: 'Férias', icon: '☼', description: 'Programação e controle de férias.' },
          { id: 'ponto', label: 'Ponto', icon: '◔', description: 'Registros de ponto por colaborador.' },
          { id: 'recrutamento', label: 'Recrutamento', icon: '◇', description: 'Vagas, candidatos e pipeline.' },
          { id: 'treinamentos', label: 'Treinamentos', icon: '◎', description: 'Portal de treinamentos, colaboradores e presença.' },
          { id: 'usuarios', label: 'Usuários', icon: '◎', description: 'Contas e perfis de acesso.' },
        ],
  );

  cadastroModules = computed<ModuleItem[]>(() => [
    { id: 'cargos', label: 'Cargos', icon: '◧', description: 'Cadastro de funções, níveis e salários base.' },
    { id: 'colaboradores', label: 'Colaboradores', icon: '◉', description: 'Cadastro completo dos colaboradores.' },
    { id: 'departamentos', label: 'Departamentos', icon: '▦', description: 'Cadastro de áreas e gestores.' },
  ]);

  peopleModules = computed<ModuleItem[]>(() => [
    { id: 'admissao', label: 'Admissão', icon: '＋', description: 'Fluxo admissional e documentos institucionais.' },
    { id: 'demissao', label: 'Demissão', icon: '−', description: 'Fluxo demissional e encerramento de vínculo.' },
  ]);

  filteredEmpresas = computed(() => this.filterByTerm(this.empresas(), ['nomeFantasia', 'razaoSocial', 'cnpj']));
  filteredContratos = computed(() => this.filterByTerm(this.contratos(), ['plano', 'status']));
  filteredUsuarios = computed(() => this.filterByTerm(this.usuarios(), ['nome', 'login', 'email', 'role']));
  filteredColaboradores = computed(() => this.filterByTerm(this.colaboradores(), ['nome', 'cpf', 'email', 'status']));
  filteredColaboradoresCadastro = computed(() => {
    const term = this.colaboradorSearch().trim().toLowerCase();
    const items = this.filteredColaboradores();
    if (!term) return items;

    return items.filter((item) => {
      const cargo = this.nomeCargo(item.cargoId);
      return [item.nome, item.cpf, item.telefone, cargo].some((value) =>
        String(value ?? '').toLowerCase().includes(term),
      );
    });
  });
  filteredVagas = computed(() => this.filterByTerm(this.vagas(), ['titulo', 'status']));
  filteredCandidatos = computed(() => this.filterByTerm(this.candidatos(), ['nome', 'email', 'status']));
  filteredCurriculos = computed(() => this.filterByTerm(this.curriculos(), ['nome', 'email', 'telefone', 'status']));
  filteredCurriculosBanco = computed(() => {
    const term = this.curriculoSearch().trim().toLowerCase();
    const items = this.filteredCurriculos();
    if (!term) return items;
    return items.filter((item) =>
      ['nome', 'telefone', 'email'].some((field) => String(item[field] ?? '').toLowerCase().includes(term)),
    );
  });

  constructor() {
    if (this.isLogged()) {
      this.carregarDados();
      this.prepararPerfil();
    }
  }

  login() {
    this.loading.set(true);
    this.http
      .post<AuthResponse>(`${this.api}/auth/login`, this.loginForm)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => this.aplicarLogin(response),
        error: () => this.notificar('Login ou senha inválidos.', 'error'),
      });
  }

  bootstrapAdmin() {
    this.loading.set(true);
    this.http
      .post<AuthResponse>(`${this.api}/auth/bootstrap-system-admin`, this.bootstrapForm)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (response) => this.aplicarLogin(response),
        error: () => this.notificar('Admin já criado ou backend indisponível.', 'error'),
      });
  }

  logout() {
    localStorage.removeItem('uniflow.token');
    localStorage.removeItem('uniflow.usuario');
    this.token.set('');
    this.usuario.set(null);
    this.empresaLogada.set(null);
    this.userMenuOpen.set(false);
    this.activeModule.set('dashboard');
    localStorage.removeItem('uniflow.empresa');
  }

  abrirPerfil() {
    this.prepararPerfil();
    this.profileModalOpen.set(true);
    this.userMenuOpen.set(false);
  }

  abrirSenha() {
    this.passwordForm = { senhaAtual: '', novaSenha: '', confirmarSenha: '' };
    this.passwordModalOpen.set(true);
    this.userMenuOpen.set(false);
  }

  salvarPerfil() {
    this.http.put<UsuarioInfo>(`${this.api}/auth/me`, this.profileForm).subscribe({
      next: (usuario) => {
        this.usuario.set(usuario);
        localStorage.setItem('uniflow.usuario', JSON.stringify(usuario));
        this.profileModalOpen.set(false);
        this.notificar('Perfil atualizado com sucesso.', 'success');
      },
      error: () => this.notificar('Não foi possível atualizar o perfil.', 'error'),
    });
  }

  alterarSenha() {
    if (this.passwordForm.novaSenha !== this.passwordForm.confirmarSenha) {
      this.notificar('A confirmação da senha não confere.', 'error');
      return;
    }

    this.http
      .post(`${this.api}/auth/change-password`, {
        senhaAtual: this.passwordForm.senhaAtual,
        novaSenha: this.passwordForm.novaSenha,
      })
      .subscribe({
        next: () => {
          this.passwordModalOpen.set(false);
          this.notificar('Senha alterada com sucesso.', 'success');
        },
        error: () => this.notificar('Não foi possível alterar a senha.', 'error'),
      });
  }

  carregarDados() {
    this.mensagem.set('');
    this.loading.set(true);

    if (this.isSistemaAdmin()) {
      this.empresaLogada.set(null);
      localStorage.removeItem('uniflow.empresa');
      this.http.get<any>(`${this.api}/contratos/dashboard`).subscribe((x) => this.metricas.set(x));
      this.http.get<Empresa[]>(`${this.api}/empresas`).subscribe((x) => this.empresas.set(x ?? []));
      this.http.get<Contrato[]>(`${this.api}/contratos`).subscribe({
        next: (x) => this.contratos.set(x ?? []),
        error: () => this.notificar('Não foi possível carregar contratos.', 'error'),
      });
      this.carregarUsuarios();
      this.loading.set(false);
      return;
    }

    this.carregarEmpresaLogada();
    this.http.get<any[]>(`${this.api}/filiais`).subscribe((x) => this.filiais.set(x ?? []));
    this.http.get<any[]>(`${this.api}/departamentos`).subscribe((x) => this.departamentos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/cargos`).subscribe((x) => this.cargos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/colaboradores`).subscribe((x) => this.colaboradores.set(x ?? []));
    this.http.get<any[]>(`${this.api}/beneficios`).subscribe((x) => this.beneficios.set(x ?? []));
    this.http.get<any[]>(`${this.api}/vagas`).subscribe((x) => this.vagas.set(x ?? []));
    this.http.get<any[]>(`${this.api}/candidatos`).subscribe((x) => this.candidatos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/curriculos`).subscribe((x) => this.curriculos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/treinamentos`).subscribe((x) => this.treinamentos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/treinamentosColaboradores`).subscribe((x) => this.treinamentosColaboradores.set(x ?? []));
    this.http.get<any[]>(`${this.api}/admissoes`).subscribe((x) => this.admissoes.set(x ?? []));
    this.http.get<any[]>(`${this.api}/demissoes`).subscribe((x) => this.demissoes.set(x ?? []));
    this.http.get<any[]>(`${this.api}/documentosInstitucionais`).subscribe((x) => this.documentosInstitucionais.set(x ?? []));
    this.http.get<any[]>(`${this.api}/ferias`).subscribe((x) => this.ferias.set(x ?? []));
    this.http.get<any[]>(`${this.api}/registrosPonto`).subscribe((x) => this.pontos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/documentosColaboradores`).subscribe((x) => this.documentos.set(x ?? []));
    this.http.get<any[]>(`${this.api}/beneficiosColaboradores`).subscribe({
      next: (x) => {
        this.beneficiosColaboradores.set(x ?? []);
        this.loading.set(false);
      },
      error: () => this.notificar('Não foi possível carregar dados da empresa.', 'error'),
    });
    this.carregarUsuarios();
  }

  carregarUsuarios() {
    this.http.get<UsuarioInfo[]>(`${this.api}/usuarios`).subscribe({
      next: (x) => this.usuarios.set(x ?? []),
      error: () => this.usuarios.set([]),
    });
  }

  carregarEmpresaLogada() {
    this.http.get<Empresa>(`${this.api}/empresas/minha`).subscribe({
      next: (empresa) => {
        this.empresaLogada.set(empresa);
        localStorage.setItem('uniflow.empresa', JSON.stringify(empresa));
      },
      error: () => {
        this.empresaLogada.set(null);
        localStorage.removeItem('uniflow.empresa');
      },
    });
  }

  selecionarFotoPerfil(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = () => {
      const result = String(reader.result ?? '');
      this.profilePhotoUrl.set(result);
      localStorage.setItem('uniflow.profilePhoto', result);
      this.notificar('Foto atualizada na barra superior.', 'success');
    };
    reader.readAsDataURL(file);
    input.value = '';
  }

  selecionarFotoColaborador(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.notificar('Selecione um arquivo de imagem.', 'error');
      input.value = '';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      this.colaboradorForm.fotoUrl = String(reader.result ?? '');
      this.notificar('Foto do colaborador carregada.', 'success');
    };
    reader.readAsDataURL(file);
    input.value = '';
  }

  salvarEmpresa() {
    this.salvarCrud<Empresa>('empresas', this.empresaForm, () => (this.empresaForm = this.novaEmpresa()));
  }

  editarEmpresa(item: Empresa) {
    this.empresaForm = { ...item };
    this.scrollTop();
  }

  salvarContrato() {
    this.salvarCrud<Contrato>('contratos', this.contratoForm, () => (this.contratoForm = this.novoContrato()));
  }

  editarContrato(item: Contrato) {
    this.contratoForm = { ...item, dataFim: this.toDateInput(item.dataFim), dataInicio: this.toDateInput(item.dataInicio) };
    this.activeModule.set('contratos');
    this.scrollTop();
  }

  salvarUsuario() {
    if (!this.isSistemaAdmin()) this.usuarioForm.empresaId = this.usuario()?.empresaId ?? undefined;

    this.http.post<UsuarioInfo>(`${this.api}/usuarios`, this.usuarioForm).subscribe({
      next: () => {
        this.usuarioForm = this.novoUsuario();
        this.carregarDados();
        this.notificar('Usuário criado com sucesso.', 'success');
      },
      error: () => this.notificar('Erro ao salvar usuário.', 'error'),
    });
  }

  salvarFilial() {
    this.salvarCrud('filiais', this.filialForm, () => (this.filialForm = this.novaFilial()));
  }

  salvarDepartamento() {
    this.salvarCrud('departamentos', this.departamentoForm, () => (this.departamentoForm = this.novoDepartamento()));
  }

  salvarCargo() {
    this.salvarCrud('cargos', this.cargoForm, () => (this.cargoForm = this.novoCargo()));
  }

  salvarColaborador() {
    this.prepararStatusColaborador();
    const editando = !!this.colaboradorForm.id;
    const payload = this.normalizePayload(this.colaboradorForm);
    const request = editando
      ? this.http.put(`${this.api}/colaboradores/${this.colaboradorForm.id}`, payload)
      : this.http.post(`${this.api}/colaboradores`, payload);

    request.subscribe({
      next: () => {
        this.colaboradorForm = this.novoColaborador();
        this.colaboradorModalOpen.set(false);
        this.colaboradoresTab.set('pesquisa');
        this.carregarDados();
        this.notificar(editando ? 'Colaborador atualizado com sucesso.' : 'Colaborador cadastrado com sucesso.', 'success');
      },
      error: () => this.notificar('Erro ao salvar colaborador.', 'error'),
    });
  }

  abrirNovoColaboradorModal() {
    this.colaboradorForm = this.novoColaborador();
    this.colaboradorModalOpen.set(true);
  }

  abrirModalColaboradorAdmissao(item?: any) {
    this.colaboradorForm = {
      ...this.novoColaborador(),
      nome: item?.nomeCandidato ?? '',
      telefone: item?.telefone ?? '',
      email: item?.email ?? '',
      cargoId: item?.cargoId,
      dataAdmissao: this.toDateInput(item?.dataPrevistaAdmissao) || new Date().toISOString().slice(0, 10),
      observacoes: item?.cargoPretendido ? `Cargo pretendido: ${item.cargoPretendido}` : '',
    };
    this.admissaoColaboradorModalOpen.set(true);
  }

  salvarColaboradorAdmissao() {
    this.prepararStatusColaborador();
    this.http.post<any>(`${this.api}/colaboradores`, this.normalizePayload(this.colaboradorForm)).subscribe({
      next: () => {
        this.colaboradorForm = this.novoColaborador();
        this.admissaoColaboradorModalOpen.set(false);
        this.carregarDados();
        this.notificar('Colaborador cadastrado pela admissão.', 'success');
      },
      error: () => this.notificar('Erro ao cadastrar colaborador pela admissão.', 'error'),
    });
  }

  editarColaborador(item: any) {
    this.colaboradorForm = {
      ...item,
      dataNascimento: this.toDateInput(item.dataNascimento),
      dataAdmissao: this.toDateInput(item.dataAdmissao),
      dataDemissao: this.toDateInput(item.dataDemissao),
    };
    this.colaboradoresTab.set('dados');
    this.scrollTop();
  }

  salvarBeneficio() {
    this.salvarCrud('beneficios', this.beneficioForm, () => (this.beneficioForm = this.novoBeneficio()));
  }

  salvarVaga() {
    this.salvarCrud('vagas', this.vagaForm, () => (this.vagaForm = this.novaVaga()));
  }

  editarVaga(item: any) {
    this.vagaForm = { ...item, dataEncerramento: this.toDateInput(item.dataEncerramento) };
    this.scrollTop();
  }

  salvarCandidato() {
    this.salvarCrud('candidatos', this.candidatoForm, () => (this.candidatoForm = this.novoCandidato()));
  }

  selecionarCurriculoArquivo(event: Event) {
    const input = event.target as HTMLInputElement;
    this.curriculoArquivo = input.files?.[0] ?? null;
  }

  salvarCurriculo() {
    const form = new FormData();
    form.append('nome', this.curriculoForm.nome ?? '');
    form.append('telefone', this.curriculoForm.telefone ?? '');
    form.append('email', this.curriculoForm.email ?? '');
    if (this.curriculoArquivo) form.append('arquivo', this.curriculoArquivo);

    this.http.post(`${this.api}/curriculos`, form).subscribe({
      next: () => {
        this.curriculoForm = this.novoCurriculo();
        this.curriculoArquivo = null;
        this.carregarDados();
        this.notificar('Currículo cadastrado com sucesso.', 'success');
      },
      error: () => this.notificar('Erro ao cadastrar currículo.', 'error'),
    });
  }

  salvarTreinamento() {
    this.normalizeDateTime(this.treinamentoForm, ['dataInicio', 'dataFim']);
    this.salvarCrud('treinamentos', this.treinamentoForm, () => (this.treinamentoForm = this.novoTreinamento()));
  }

  vincularTreinamentoColaborador() {
    this.http.post(`${this.api}/treinamentosColaboradores`, this.normalizePayload(this.treinamentoColaboradorForm)).subscribe({
      next: () => {
        this.treinamentoColaboradorForm = this.novoTreinamentoColaborador();
        this.carregarDados();
        this.notificar('Colaborador vinculado ao treinamento.', 'success');
      },
      error: () => this.notificar('Erro ao vincular colaborador ao treinamento.', 'error'),
    });
  }

  marcarPresenca(item: any, presente: boolean) {
    this.http
      .put(`${this.api}/treinamentosColaboradores/${item.id}/presenca`, { ...item, presente, status: presente ? 'Presente' : 'Ausente' })
      .subscribe({
        next: () => {
          this.carregarDados();
          this.notificar('Presença atualizada.', 'success');
        },
        error: () => this.notificar('Erro ao atualizar presença.', 'error'),
      });
  }

  salvarAdmissao() {
    this.normalizeDateTime(this.admissaoForm, ['dataPrevistaAdmissao']);
    this.salvarCrud('admissoes', this.admissaoForm, () => (this.admissaoForm = this.novaAdmissao()));
  }

  admitirColaborador(item: any) {
    this.http.post(`${this.api}/admissoes/${item.id}/admitir`, {}).subscribe({
      next: () => {
        this.carregarDados();
        this.notificar('Colaborador admitido e documentos institucionais gerados.', 'success');
      },
      error: () => this.notificar('Erro ao concluir admissão.', 'error'),
    });
  }

  salvarDemissao() {
    this.normalizeDateTime(this.demissaoForm, ['dataSolicitacao', 'dataDesligamento']);
    this.salvarCrud('demissoes', this.demissaoForm, () => (this.demissaoForm = this.novaDemissao()));
  }

  concluirDemissao(item: any) {
    this.http.post(`${this.api}/demissoes/${item.id}/concluir`, {}).subscribe({
      next: () => {
        this.carregarDados();
        this.notificar('Fluxo demissional concluído e colaborador inativado.', 'success');
      },
      error: () => this.notificar('Erro ao concluir demissão.', 'error'),
    });
  }

  salvarFerias() {
    this.salvarCrud('ferias', this.feriasForm, () => (this.feriasForm = this.novaFerias()));
  }

  salvarPonto() {
    this.pontoForm.dataHora = new Date(this.pontoForm.dataHora).toISOString();
    this.http.post(`${this.api}/registrosPonto`, this.pontoForm).subscribe({
      next: () => {
        this.pontoForm = this.novoPonto();
        this.carregarDados();
        this.notificar('Registro de ponto lançado.', 'success');
      },
      error: () => this.notificar('Erro ao lançar ponto.', 'error'),
    });
  }

  salvarDocumento() {
    this.http.post(`${this.api}/documentosColaboradores`, this.documentoForm).subscribe({
      next: () => {
        this.documentoForm = this.novoDocumento();
        this.carregarDados();
        this.notificar('Documento salvo.', 'success');
      },
      error: () => this.notificar('Erro ao salvar documento.', 'error'),
    });
  }

  salvarBeneficioColaborador() {
    this.http.post(`${this.api}/beneficiosColaboradores`, this.normalizePayload(this.beneficioColaboradorForm)).subscribe({
      next: () => {
        this.beneficioColaboradorForm = this.novoBeneficioColaborador();
        this.carregarDados();
        this.notificar('Benefício vinculado ao colaborador.', 'success');
      },
      error: () => this.notificar('Erro ao vincular benefício.', 'error'),
    });
  }

  editarGenerico(form: string, item: any) {
    (this as any)[form] = { ...item };
    this.scrollTop();
  }

  pedirExclusao(endpoint: string, id: number | undefined, label: string) {
    if (!id) return;
    this.confirmDelete.set({ endpoint, id, label });
  }

  confirmarExclusao() {
    const item = this.confirmDelete();
    if (!item) return;

    this.http.delete(`${this.api}/${item.endpoint}/${item.id}`).subscribe({
      next: () => {
        this.confirmDelete.set(null);
        this.carregarDados();
        this.notificar('Registro excluído.', 'success');
      },
      error: () => this.notificar('Não foi possível excluir o registro.', 'error'),
    });
  }

  nomeEmpresa(id?: number) {
    return this.empresas().find((x) => x.id === id)?.nomeFantasia ?? id ?? '-';
  }

  nomeColaborador(id?: number) {
    return this.colaboradores().find((x) => x.id === id)?.nome ?? id ?? '-';
  }

  nomeCargo(id?: number) {
    return this.cargos().find((x) => x.id === id)?.nome ?? id ?? '-';
  }

  nomeVaga(id?: number) {
    return this.vagas().find((x) => x.id === id)?.titulo ?? id ?? '-';
  }

  nomeBeneficio(id?: number) {
    return this.beneficios().find((x) => x.id === id)?.nome ?? id ?? '-';
  }

  nomeTreinamento(id?: number) {
    return this.treinamentos().find((x) => x.id === id)?.titulo ?? id ?? '-';
  }

  setModule(id: string) {
    this.activeModule.set(id);
    if (this.cadastroModules().some((module) => module.id === id)) this.cadastroMenuOpen.set(true);
    if (id === 'admissao' || id === 'demissao') this.peopleMenuOpen.set(true);
    this.search.set('');
    this.userMenuOpen.set(false);
  }

  private salvarCrud<T extends { id?: number }>(endpoint: string, payload: T, reset: () => void) {
    const body = this.normalizePayload(payload);
    const request = payload.id
      ? this.http.put(`${this.api}/${endpoint}/${payload.id}`, body)
      : this.http.post(`${this.api}/${endpoint}`, body);

    request.subscribe({
      next: () => {
        reset();
        this.carregarDados();
        this.notificar(payload.id ? 'Registro atualizado com sucesso.' : 'Registro criado com sucesso.', 'success');
      },
      error: () => this.notificar(`Erro ao salvar ${endpoint}.`, 'error'),
    });
  }

  private aplicarLogin(response: AuthResponse) {
    localStorage.setItem('uniflow.token', response.token);
    localStorage.setItem('uniflow.usuario', JSON.stringify(response.usuario));
    this.token.set(response.token);
    this.usuario.set(response.usuario);
    this.prepararPerfil();
    this.carregarDados();
  }

  private prepararPerfil() {
    const usuario = this.usuario();
    this.profileForm = { nome: usuario?.nome ?? '', email: usuario?.email ?? '' };
  }

  private notificar(texto: string, tipo: MessageType = 'info') {
    this.mensagem.set(texto);
    this.mensagemTipo.set(tipo);
    setTimeout(() => {
      if (this.mensagem() === texto) this.mensagem.set('');
    }, 4200);
  }

  private filterByTerm<T extends Record<string, any>>(items: T[], fields: string[]) {
    const term = this.search().trim().toLowerCase();
    if (!term) return items;
    return items.filter((item) =>
      fields.some((field) => String(item[field] ?? '').toLowerCase().includes(term)),
    );
  }

  private scrollTop() {
    setTimeout(() => document.querySelector('.workspace')?.scrollTo({ top: 0, behavior: 'smooth' }), 0);
  }

  private toDateInput(value: any) {
    return value ? new Date(value).toISOString().slice(0, 10) : '';
  }

  private normalizePayload<T>(payload: T): T {
    return JSON.parse(
      JSON.stringify(payload, (_key, value) => {
        if (value === '') return null;
        return value;
      }),
    ) as T;
  }

  private normalizeDateTime(payload: any, fields: string[]) {
    for (const field of fields) {
      if (payload[field]) payload[field] = new Date(payload[field]).toISOString();
    }
  }

  private prepararStatusColaborador() {
    this.colaboradorForm.ativo = this.colaboradorForm.status === 'Ativo';
    if (this.colaboradorForm.status === 'Demitido' && !this.colaboradorForm.dataDemissao) {
      this.colaboradorForm.dataDemissao = new Date().toISOString().slice(0, 10);
    }
    if (this.colaboradorForm.status === 'Ativo') {
      this.colaboradorForm.dataDemissao = '';
    }
  }

  private novaEmpresa(): Empresa {
    return {
      razaoSocial: '',
      nomeFantasia: '',
      cnpj: '',
      telefone: '',
      email: '',
      endereco: '',
      cidade: '',
      estado: '',
      cep: '',
      ativo: true,
    };
  }

  private novoContrato(): Contrato {
    return {
      empresaId: 0,
      plano: 'Starter',
      status: 'Ativo',
      dataInicio: new Date().toISOString().slice(0, 10),
      dataFim: '',
      limiteColaboradores: 50,
      valorMensal: 0,
      observacoes: '',
    };
  }

  private novoUsuario(): UsuarioCreate {
    return {
      empresaId: undefined,
      colaboradorId: undefined,
      nome: '',
      login: '',
      email: '',
      senha: '',
      role: 'EmpresaAdmin',
      ativo: true,
    };
  }

  private novaFilial() {
    return { nome: '', cnpj: '', endereco: '', cidade: '', estado: '', telefone: '', ativa: true };
  }

  private novoDepartamento() {
    return { nome: '', descricao: '', gestorId: undefined, ativo: true };
  }

  private novoCargo() {
    return { nome: '', descricao: '', nivel: '', salarioBase: 0, ativo: true };
  }

  private novoColaborador() {
    return {
      filialId: undefined,
      departamentoId: undefined,
      cargoId: undefined,
      nome: '',
      cpf: '',
      rg: '',
      telefone: '',
      email: '',
      endereco: '',
      bairro: '',
      cidade: '',
      estado: '',
      cep: '',
      dataNascimento: '',
      sexo: '',
      estadoCivil: '',
      dataAdmissao: new Date().toISOString().slice(0, 10),
      dataDemissao: '',
      tipoContrato: 'CLT',
      salario: 0,
      cargaHorariaSemanal: 44,
      matricula: '',
      pis: '',
      ctps: '',
      status: 'Ativo',
      fotoUrl: '',
      observacoes: '',
      ativo: true,
    };
  }

  private novoBeneficio() {
    return { nome: '', descricao: '', valorPadrao: 0, ativo: true };
  }

  private novaVaga() {
    return {
      departamentoId: undefined,
      cargoId: undefined,
      titulo: '',
      descricao: '',
      quantidade: 1,
      salario: 0,
      status: 'Aberta',
      dataEncerramento: '',
    };
  }

  private novoCandidato() {
    return {
      vagaId: undefined,
      nome: '',
      cpf: '',
      telefone: '',
      email: '',
      curriculoUrl: '',
      linkedin: '',
      status: 'Recebido',
      observacoes: '',
    };
  }

  private novoCurriculo() {
    return {
      nome: '',
      telefone: '',
      email: '',
    };
  }

  private novoTreinamento() {
    return {
      titulo: '',
      descricao: '',
      instrutor: '',
      dataInicio: new Date().toISOString().slice(0, 16),
      dataFim: '',
      cargaHoraria: 1,
      status: 'Planejado',
      obrigatorio: false,
    };
  }

  private novoTreinamentoColaborador() {
    return {
      treinamentoId: undefined,
      colaboradorId: undefined,
      presente: false,
      status: 'Inscrito',
      observacoes: '',
    };
  }

  private novaAdmissao() {
    return {
      colaboradorId: undefined,
      nomeCandidato: '',
      email: '',
      telefone: '',
      cargoPretendido: '',
      dataPrevistaAdmissao: new Date().toISOString().slice(0, 10),
      status: 'Em andamento',
    };
  }

  private novaDemissao() {
    return {
      colaboradorId: undefined,
      tipoDemissao: 'Sem justa causa',
      dataSolicitacao: new Date().toISOString().slice(0, 10),
      dataDesligamento: '',
      status: 'Em andamento',
      motivo: '',
      observacoes: '',
    };
  }

  private novaFerias() {
    const hoje = new Date().toISOString().slice(0, 10);
    return {
      colaboradorId: undefined,
      periodoAquisitivoInicio: hoje,
      periodoAquisitivoFim: hoje,
      dataInicio: hoje,
      dataFim: hoje,
      dias: 30,
      abono: false,
      status: 'Pendente',
    };
  }

  private novoPonto() {
    return {
      colaboradorId: undefined,
      dataHora: new Date().toISOString().slice(0, 16),
      tipo: 'Entrada',
      latitude: undefined,
      longitude: undefined,
      ip: '',
      observacao: '',
    };
  }

  private novoDocumento() {
    return {
      colaboradorId: undefined,
      tipoDocumento: '',
      nomeArquivo: '',
      urlArquivo: '',
      obrigatorio: false,
      validado: false,
    };
  }

  private novoBeneficioColaborador() {
    return {
      colaboradorId: undefined,
      beneficioId: undefined,
      valor: 0,
      dataInicio: new Date().toISOString().slice(0, 10),
      dataFim: '',
      ativo: true,
    };
  }
}
